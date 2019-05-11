﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BattleTech;
using EnhancedAI.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EnhancedAI
{
    public class BehaviorNodeJSONRepresentation
    {
        [JsonRequired]
        public string Name { get; set; }

        [JsonRequired]
        public string TypeName { get; set; }

        public List<BehaviorNodeJSONRepresentation> Children { get; set; }
        public Dictionary<string, object> ExtraParameters { get; set; }


        private object[] GetNodeConstructorParameters(BehaviorTree tree, AbstractActor unit, out Type[] parameterTypes)
        {
            var parameters = new List<object> { Name, tree, unit };
            var parameterTypeList = new List<Type> { typeof(string), typeof(BehaviorTree), typeof(AbstractActor) };

            if (ExtraParameters == null || ExtraParameters.Count == 0)
            {
                parameterTypes = parameterTypeList.ToArray();
                return parameters.ToArray();
            }

            var extraParameterInfo = NodeUtil.GetConstructorExtraParameterInfo(TypeName);
            parameterTypeList.AddRange(extraParameterInfo.Select(parameterInfo => parameterInfo.ParameterType));
            parameters.AddRange(extraParameterInfo.Select(parameterInfo => ExtraParameters[parameterInfo.Name]));

            // for some reason unity/newtonsoft json doesn't seem to like converting
            // string -> enum while the tests seem to work just fine
            // this causes hang on mission load without any sort of exception or anything
            for (var i = 0; i < parameterTypeList.Count; i++)
            {
                var parameterType = parameterTypeList[i];
                if (parameterType.IsEnum && parameters[i] is string parameter)
                {
                    parameters[i] = Enum.Parse(parameterTypeList[i], parameter);
                }
            }

            parameterTypes = parameterTypeList.ToArray();
            return parameters.ToArray();
        }

        public BehaviorNode ToNode(BehaviorTree tree, AbstractActor unit)
        {
            var parameters = GetNodeConstructorParameters(tree, unit, out var parameterTypes);
            var node = BehaviorNodeFactory.CreateBehaviorNode(TypeName, parameterTypes, parameters);

            if (node == null)
                Main.HBSLog?.LogError($"BehaviorNode name: {Name} type: {TypeName} did not convert properly!");

            switch (node)
            {
                case CompositeBehaviorNode composite:
                    foreach (var child in Children)
                        composite.AddChild(child.ToNode(tree, unit));
                    break;

                case DecoratorBehaviorNode decorator:
                    decorator.ChildNode = Children.FirstOrDefault()?.ToNode(tree, unit);
                    break;
            }

            return node;
        }


        private void ToJSON(TextWriter textWriter, Formatting formatting)
        {
            using (var writer = new JsonTextWriter(textWriter))
            {
                var serializer = new JsonSerializer
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    Formatting = formatting
                };
                serializer.Converters.Add(new StringEnumConverter());

                serializer.Serialize(writer, this);
            }
        }

        public void ToJSONFile(string path)
        {
            using (var textWriter = File.CreateText(path))
            {
                ToJSON(textWriter, Formatting.Indented);
            }
        }

        public string ToJSONString()
        {
            using (var textWriter = new StringWriter())
            {
                ToJSON(textWriter, Formatting.Indented);
                return textWriter.ToString();
            }
        }


        public static BehaviorNodeJSONRepresentation FromNode(BehaviorNode node)
        {
            var rep = new BehaviorNodeJSONRepresentation();
            rep.Name = node.GetName();
            rep.TypeName = node.GetType().ToString();

            switch (node)
            {
                case CompositeBehaviorNode composite:
                    rep.Children = new List<BehaviorNodeJSONRepresentation>();
                    foreach (var child in composite.Children)
                        rep.Children.Add(FromNode(child));
                    break;

                case DecoratorBehaviorNode decorator:
                    rep.Children = new List<BehaviorNodeJSONRepresentation> { FromNode(decorator.ChildNode) };
                    break;

                default:
                    rep.Children = null;
                    break;
            }

            var extraParameterInfo = NodeUtil.GetConstructorExtraParameterInfo(node.GetType());
            if (extraParameterInfo == null || extraParameterInfo.Length == 0)
                return rep;

            rep.ExtraParameters = new Dictionary<string, object>();
            foreach (var parameterInfo in extraParameterInfo)
            {
                var value = NodeUtil.GetParameterValueByType(node, parameterInfo.ParameterType);
                rep.ExtraParameters.Add(parameterInfo.Name, value);
            }

            return rep;
        }

        public static BehaviorNodeJSONRepresentation FromJSON(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<BehaviorNodeJSONRepresentation>(json);
            }
            catch (Exception e)
            {
                Main.HBSLog?.LogError("BehaviorNodeJSONRepresentation.FromJSON tossed exception");
                Main.HBSLog?.LogException(e);
                return null;
            }
        }

        public static BehaviorNodeJSONRepresentation FromPath(string path)
        {
            if (!File.Exists(path))
            {
                Main.HBSLog?.LogWarning($"Could not find file at: {path}");
                return null;
            }

            return FromJSON(File.ReadAllText(path));
        }
    }
}
