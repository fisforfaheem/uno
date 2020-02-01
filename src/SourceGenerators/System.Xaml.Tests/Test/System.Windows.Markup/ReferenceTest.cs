//
// Copyright (C) 2010 Novell Inc. http://novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using Uno.Xaml;
using Uno.Xaml.Schema;
using NUnit.Framework;

using Category = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Markup
{
	[TestFixture]
	public class ReferenceTest
	{
		[Test]
		public void ConstructorNullName ()
		{
			new Reference ((string) null); // it is somehow allowed
		}

		[Test]
		public void ProvideValueWithoutTypeOrName ()
		{
			Assert.Throws(typeof(ArgumentNullException), () =>
			{
				new Reference().ProvideValue(null);
			});
		}

		[Test]
		public void ProvideValueWithNameWithoutResolver ()
		{
			Assert.Throws(typeof(ArgumentNullException), () =>
			{
				var x = new Reference("X");
				x.ProvideValue(null); // serviceProvider is required.
			});
		}

		[Test]
		public void ProvideValueWithNameWithProviderNoResolver ()
		{
			Assert.Throws(typeof(InvalidOperationException), () =>
			{
				var x = new Reference("X");
				x.ProvideValue(new NameServiceProvider(false, false));
			});
		}

		[Test]
		public void ProvideValueWithNameWithProviderResolveFail ()
		{
			var x = new Reference ("X");
			var r = new NameServiceProvider (true, false);
			Assert.AreEqual ("BAR", x.ProvideValue (r), "#1");
		}

		[Test]
		public void ProvideValueWithNameWithProviderResolveSuccess ()
		{
			var x = new Reference ("Y");
			var r = new NameServiceProvider (true, true);
			Assert.AreEqual ("FOO", x.ProvideValue (r), "#1");
		}

		private class NameServiceProvider : IServiceProvider
		{
			private readonly Resolver resolver;

			public NameServiceProvider (bool worksFine, bool resolvesFine)
			{
				resolver = worksFine ? new Resolver (resolvesFine) : null;
			}

			public object GetService (Type serviceType)
			{
				Assert.AreEqual (typeof (IXamlNameResolver), serviceType, "TypeToResolve");
				return resolver;
			}
		}

		private class Resolver : IXamlNameResolver
		{
			private readonly bool resolves;

			public Resolver (bool resolvesFine)
			{
				resolves = resolvesFine;
			}

			public IEnumerable<KeyValuePair<string, object>> GetAllNamesAndValuesInScope ()
			{
				throw new Exception ();
			}
			
			public object GetFixupToken (IEnumerable<string> names)
			{
				throw new NotImplementedException ();
			}
			
			// only X (which 'failed' to resolve) calls this
			public object GetFixupToken (IEnumerable<string> names, bool canAssignDirectly)
			{
				Assert.IsTrue (canAssignDirectly, "canAssignDirectly");
				Assert.AreEqual (1, names.Count (), "Count");
				Assert.AreEqual ("X", names.First (), "name0");
				return "BAR";
			}
			
			public bool IsFixupTokenAvailable {
				get { throw new NotImplementedException (); }
			}
			
			public event EventHandler OnNameScopeInitializationComplete;

			// both X and Y calls this.
			public object Resolve (string name)
			{
				return resolves ? "FOO" : null;
			}

			public object Resolve (string name, out bool isFullyInitialized)
			{
				throw new NotImplementedException ();
			}
		}
	}
}
