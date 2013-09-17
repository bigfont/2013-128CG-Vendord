using System;

namespace System.Runtime.InteropServices
{
	/// <summary>
	/// IntPtr wrapper which can be used as result of
	/// Marshal.AllocHGlobal operation.
	/// Call Marshal.FreeHGlobal when disposed or finalized.
	/// </summary>
	class HGlobalSafeHandle : SafeHandle
	{
		/// <summary>
		/// Creates new instance with given IntPtr value
		/// </summary>
		public HGlobalSafeHandle(IntPtr ptr) : base(ptr, true)
		{
		}

		/// <summary>
		/// Creates new instance with zero IntPtr
		/// </summary>
		public HGlobalSafeHandle() : base(IntPtr.Zero, true)
		{
		}

		/// <summary>
		/// Creates new instance which allocates unmanaged memory of given size
		/// </summary>
		public HGlobalSafeHandle(int size) : base(Marshal.AllocHGlobal(size), true)
		{
		}

		/// <summary>
		/// Creates a new instance and copies the bytes into the allocated unmanaged memory.
		/// </summary>
		/// <param name="buffer">Bytes to copy</param>
		public HGlobalSafeHandle(byte[] buffer) : this(buffer.Length)
		{
			Marshal.Copy(buffer, 0, this.handle, buffer.Length);
		}

		/// <summary>
		/// Allows to assign IntPtr to HGlobalSafeHandle
		/// </summary>
		public static implicit operator HGlobalSafeHandle(IntPtr ptr)
		{
			return new HGlobalSafeHandle(ptr);
		}

		/// <summary>
		/// Allows to use HGlobalSafeHandle as IntPtr
		/// </summary>
		public static implicit operator IntPtr(HGlobalSafeHandle h)
		{
			return h.handle;
		}

		[DllImport("kernel32", SetLastError = true)]
		private static extern uint LocalSize(IntPtr hMem);

		/// <summary>
		/// Gets or sets the length of the unmanaged memory. Setting a new value will reallocate the local memory.
		/// </summary>
		public uint Length
		{
			get
			{
				return LocalSize(this.handle);
			}
			set
			{
				base.SetHandle(Marshal.ReAllocHGlobal(this.handle, new IntPtr(value)));
			}
		}

		/// <summary>
		/// Allows safe extraction to an array of managed bytes.
		/// </summary>
		/// <param name="h">Reference to an HGlobalSafeHandle</param>
		/// <returns>Managed array of bytes.</returns>
		public static implicit operator byte[](HGlobalSafeHandle h)
		{
			uint sz = h.Length;
			byte[] buffer = new byte[sz];
			Marshal.Copy(h.handle, buffer, 0, (int)sz);
			return buffer;
		}

		/// <summary>
		/// Called when object is disposed or finalized.
		/// </summary>
		override protected bool ReleaseHandle()
		{
			Marshal.FreeHGlobal(handle);
			return true;
		}

		/// <summary>
		/// Defines invalid (null) handle value.
		/// </summary>
		public override bool IsInvalid { get { return (handle == IntPtr.Zero); } }

		/// <summary>
		/// Returns the memory cast to a string.
		/// </summary>
		/// <returns>String value of memory.</returns>
		public override string ToString()
		{
			try
			{
				return System.Text.Encoding.Unicode.GetString((byte[])this);
			}
			catch {}
			return string.Empty;
		}
	}
}
