﻿// Copyright © Syterra Software Inc. All rights reserved.
// The use and distribution terms for this software are covered by the Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file license.txt at the root of this distribution. By using this software in any fashion, you are agreeing
// to be bound by the terms of this license. You must not remove this notice, or any other, from this software.

using System;
using fitSharp.Machine.Model;

namespace fitSharp.Machine.Engine {
    public class DefaultParse<T>: ParseOperator<T> {
        public bool TryParse(Processor<T> processor, Type type, TypedValue instance, Tree<T> parameters, ref TypedValue result) {
            if (type.IsAssignableFrom(typeof(string))) {
                result = new TypedValue(parameters.Value.ToString(), typeof(string));
                return true;
            }
            RuntimeMember parse = new RuntimeType(type).FindStatic("parse", new[] {typeof (string)});
            if (parse != null && parse.ReturnType == type) {
                result = parse.Invoke(new object[] {parameters.Value.ToString()});
                return true;
            }

            RuntimeMember construct = new RuntimeType(type).FindConstructor(new[] {typeof (string)});
            if (construct != null) {
                result = construct.Invoke(new object[] {parameters.Value.ToString()});
                return true;
            }

            throw new InvalidOperationException(
                string.Format("Can't parse {0} because it doesn't have a static Parse method", type.FullName));
        }
    }
}