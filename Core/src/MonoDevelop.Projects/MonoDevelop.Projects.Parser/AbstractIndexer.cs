// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version value="$version"/>
// </file>
using System;
using MonoDevelop.Projects.Utility;

namespace MonoDevelop.Projects.Parser
{
	[Serializable]
	public abstract class AbstractIndexer : AbstractMember, IIndexer
	{
		protected IRegion             bodyRegion;
		protected IRegion             getterRegion;
		protected IRegion             setterRegion;
		protected ParameterCollection parameters = new ParameterCollection();
		
		public virtual IRegion BodyRegion {
			get {
				return bodyRegion;
			}
		}


		public IRegion GetterRegion {
			get {
				return getterRegion;
			}
		}

		public IRegion SetterRegion {
			get {
				return setterRegion;
			}
		}

		public virtual ParameterCollection Parameters {
			get {
				return parameters;
			}
		}
		
		public override int CompareTo (object ob) 
		{
			int cmp;
			IIndexer value = (IIndexer) ob;
			
			cmp = base.CompareTo (value);
			if (cmp != 0) {
				return cmp;
			}
			
			if (GetterRegion != null) {
				if (value.GetterRegion == null)
					return -1;
				cmp = GetterRegion.CompareTo(value.GetterRegion);
				if (cmp != 0) {
					return cmp;
				}
			} else if (value.GetterRegion != null)
				return 1;
			
			if (SetterRegion != null) {
				if (value.SetterRegion == null)
					return -1;
				cmp = SetterRegion.CompareTo(value.SetterRegion);
				if (cmp != 0) {
					return cmp;
				}
			} else if (value.SetterRegion != null)
				return 1;
			
			return DiffUtility.Compare(Parameters, value.Parameters);
		}
		
		public override bool Equals (object ob)
		{
			IIndexer other = ob as IIndexer;
			if (other == null) return false;
			return CompareTo (other) == 0;
		}
		
		public override int GetHashCode ()
		{
			int c = base.GetHashCode ();
			if (GetterRegion != null)
				c += GetterRegion.GetHashCode ();
			if (SetterRegion != null)
				c += SetterRegion.GetHashCode ();
			return c;
		}
	}
}
