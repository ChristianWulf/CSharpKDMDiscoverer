using System;
namespace System
{
	internal sealed class LocalDataStoreHolder
	{
		private LocalDataStore m_Store;
		public LocalDataStore Store
		{
			get
			{
				return this.m_Store;
			}
		}
		public LocalDataStoreHolder(LocalDataStore store)
		{
			this.m_Store = store;
		}
		protected override void Finalize()
		{
			try
			{
				LocalDataStore store = this.m_Store;
				if (store != null)
				{
					store.Dispose();
				}
			}
			finally
			{
				base.Finalize();
			}
		}
	}
}
