// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.MSBuild
{
    /// <summary>
    /// Represents a SectionBlock in a .sln file. Section blocks are of the form:
    /// 
    /// Type(ParenthesizedName) = Value
    ///     Key = Value
    ///     [more keys/values]
    /// EndType
    /// </summary>
    public class SectionBlock
    {
        private readonly string type;
        private readonly string parenthesizedName;
        private readonly string value;
        private readonly IEnumerable<KeyValuePair<string, string>> keyValuePairs;

        public SectionBlock(string type, string parenthesizedName, string value, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
        {
            if (string.IsNullOrEmpty(type))
            {
                //throw new ArgumentException(string.Format(WorkspacesResources.StringIsNullOrEmpty, "type"));
                throw new ArgumentException();
            }

            if (string.IsNullOrEmpty(parenthesizedName))
            {
                //throw new ArgumentException(string.Format(WorkspacesResources.StringIsNullOrEmpty, "parenthesizedName"));
                throw new ArgumentException();
            }

            if (string.IsNullOrEmpty(value))
            {
                //throw new ArgumentException(string.Format(WorkspacesResources.StringIsNullOrEmpty, "value"));
                throw new ArgumentException();
            }

            this.type = type;
            this.parenthesizedName = parenthesizedName;
            this.value = value;
            this.keyValuePairs = keyValuePairs.ToList();
        }

        public string Type
        {
            get { return type; }
        }

        public string ParenthesizedName
        {
            get { return parenthesizedName; }
        }

        public string Value
        {
            get { return value; }
        }

        public IEnumerable<KeyValuePair<string, string>> KeyValuePairs
        {
            get { return keyValuePairs; }
        }

        internal string GetText(int indent)
        {
            var builder = new StringBuilder();

            builder.Append('\t', indent);
            builder.AppendFormat("{0}({1}) = ", Type, ParenthesizedName);
            builder.AppendLine(Value);

            foreach (var pair in KeyValuePairs)
            {
                builder.Append('\t', indent + 1);
                builder.Append(pair.Key);
                builder.Append(" = ");
                builder.AppendLine(pair.Value);
            }

            builder.Append('\t', indent);
            builder.AppendFormat("End{0}", Type);
            builder.AppendLine();

            return builder.ToString();
        }

        internal static SectionBlock Parse(TextReader reader)
        {
            string startLine;
            while ((startLine = reader.ReadLine()) != null)
            {
                startLine = startLine.TrimStart(null);
                if (startLine != string.Empty)
                {
                    break;
                }
            }

            var scanner = new LineScanner(startLine);

            var type = scanner.ReadUpToAndEat("(");
            var parenthesizedName = scanner.ReadUpToAndEat(") = ");
            var sectionValue = scanner.ReadRest();

            var keyValuePairs = new List<KeyValuePair<string, string>>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.TrimStart(null);

                // ignore empty lines
                if (line == string.Empty)
                {
                    continue;
                }

                if (line == "End" + type)
                {
                    break;
                }

                scanner = new LineScanner(line);
                var key = scanner.ReadUpToAndEat(" = ");
                var value = scanner.ReadRest();

                keyValuePairs.Add(new KeyValuePair<string, string>(key, value));
            }

            return new SectionBlock(type, parenthesizedName, sectionValue, keyValuePairs);
        }
    }
}