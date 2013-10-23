using System.IO;

namespace System.Devices
{
	/// <summary>
	/// Exposes a Stream around a remote file, supporting synchronous read and write operations. 
	/// </summary>
	public class RemoteFileStream : System.IO.Stream
	{
		internal const int DefaultBufferSize = 0x80;

		private long _appendStart;
		private byte[] _buffer;
		private int _bufferSize;
		private bool _canRead;
		private bool _canSeek;
		private bool _canWrite;
		private ulong _pos;
		private int _readLen;
		private int _readPos;
		private int _writePos;
		private RemoteDevice.DeviceFile f;

		/// <summary>
		/// Initializes a new instance of the <see cref="RemoteFileStream"/> class with the specified path and creation mode.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">A relative or absolute path for the file that the current <see cref="RemoteFileStream"/> object will encapsulate.</param>
		/// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
		public RemoteFileStream(RemoteDevice device, string path, FileMode mode)
			: this(device, path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.Read, FileAttributes.Normal)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RemoteFileStream"/> class with the specified path, creation mode, and read/write permission.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">A relative or absolute path for the file that the current <see cref="RemoteFileStream"/> object will encapsulate.</param>
		/// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
		/// <param name="access">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
		public RemoteFileStream(RemoteDevice device, string path, FileMode mode, FileAccess access)
			: this(device, path, mode, access, FileShare.Read, FileAttributes.Normal)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RemoteFileStream"/> class with the specified path, creation mode, read/write permission, and sharing permission.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">A relative or absolute path for the file that the current <see cref="RemoteFileStream"/> object will encapsulate.</param>
		/// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
		/// <param name="access">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
		/// <param name="share">A <see cref="FileShare"/> value specifying the type of access other threads have to the file.</param>
		public RemoteFileStream(RemoteDevice device, string path, FileMode mode, FileAccess access, FileShare share)
			: this(device, path, mode, access, share, FileAttributes.Normal)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RemoteFileStream"/> class with the specified path, creation mode, read/write permission, sharing permission, and attributes.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="path">A relative or absolute path for the file that the current <see cref="RemoteFileStream"/> object will encapsulate.</param>
		/// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
		/// <param name="access">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
		/// <param name="share">A <see cref="FileShare"/> value specifying the type of access other threads have to the file.</param>
		/// <param name="attributes">The <see cref="FileAttributes"/> to set on the new file.</param>
		public RemoteFileStream(RemoteDevice device, string path, FileMode mode, FileAccess access, FileShare share, FileAttributes attributes)
		{
			f = new RemoteDevice.DeviceFile(device.ISession, path, ((uint)access << 30), (uint)share, (uint)mode, (uint)attributes);
			this._canRead = (access & FileAccess.Read) != 0;
			this._canWrite = (access & FileAccess.Write) != 0;
			this._canSeek = true;
			this._pos = 0L;
			this._bufferSize = DefaultBufferSize;
			this._readPos = 0;
			this._readLen = 0;
			this._writePos = 0;
			if (mode == FileMode.Append)
				this._appendStart = this.SeekCore(0L, SeekOrigin.End);
			else
				this._appendStart = -1L;
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports reading.
		/// </summary>
		/// <value></value>
		/// <returns><c>true</c> if the stream supports reading; <c>false</c> if the stream is closed or was opened with write-only permissions.</returns>
		public override bool CanRead
		{
			get { return _canRead; }
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports seeking.
		/// </summary>
		/// <value></value>
		/// <returns><c>true</c> if the stream supports seeking; <c>false</c> if the stream is closed or if the <see cref="RemoteFileStream"/> was constructed from an operating-system handle such as a pipe or output to the console.</returns>
		public override bool CanSeek
		{
			get { return _canSeek; }
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports writing.
		/// </summary>
		/// <value></value>
		/// <returns><c>true</c> if the stream supports writing; <c>false</c> if the stream is closed or was opened with read-only access.</returns>
		public override bool CanWrite
		{
			get { return _canWrite; }
		}

		/// <summary>
		/// Gets the length in bytes of the stream.
		/// </summary>
		/// <value></value>
		/// <returns>A long value representing the length of the stream in bytes.</returns>
		public override long Length
		{
			get
			{
				if (!this.CanSeek)
					throw new NotSupportedException();

				ulong l = f.Size;
				if ((this._writePos > 0) && ((this._pos + (ulong)this._writePos) > l))
				{
					l = (ulong)this._writePos + this._pos;
				}
				return (long)l;
			}
		}

		/// <summary>
		/// Gets the name of the file that was passed to the constructor.
		/// </summary>
		/// <value>The name of the file passed to the constructor.</value>
		public string Name { get { return f.Name; } }

		/// <summary>
		/// Gets or sets the position within the current stream.
		/// </summary>
		/// <value></value>
		/// <returns>The current position within the stream.</returns>
		public override long Position
		{
			get
			{
				if (!this.CanSeek)
					throw new NotSupportedException();
				this.VerifyOSHandlePosition();
				return (long)(this._pos + (ulong)((this._readPos - this._readLen) + this._writePos));
			}
			set
			{
				if (value < 0L)
					throw new ArgumentOutOfRangeException();
				if (this._writePos > 0)
					this.FlushWrite();
				this._readPos = 0;
				this._readLen = 0;
				this.Seek(value, SeekOrigin.Begin);
			}
		}

		internal int BufferSize
		{
			get { return _bufferSize; }
			set { Flush(); _bufferSize = value; this._buffer = null; }
		}

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
		/// </summary>
		public override void Flush()
		{
			if (this._writePos > 0)
			{
				this.FlushWrite();
				f.SetEndOfFile();
			}
			else if ((this._readPos < this._readLen) && this.CanSeek)
			{
				this.FlushRead();
			}
		}

		/// <summary>
		/// Reads a block of bytes from the stream and writes the data in a given buffer.
		/// </summary>
		/// <param name="array">When this method returns, contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source. </param>
		/// <param name="offset">The byte offset in <paramref name="array"/> at which the read bytes will be placed.</param>
		/// <param name="count">The maximum number of bytes to read.</param>
		/// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
		public override int Read(byte[] array, int offset, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException();
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if ((array.Length - offset) < count)
			{
				throw new ArgumentException();
			}
			bool flag = false;
			int num = this._readLen - this._readPos;
			if (num == 0)
			{
				if (!this.CanRead)
				{
					throw new NotSupportedException();
				}
				if (this._writePos > 0)
				{
					this.FlushWrite();
				}
				if (count >= this._bufferSize)
				{
					return this.ReadCore(array, offset, count);
				}
				if (this._buffer == null)
				{
					this._buffer = new byte[this._bufferSize];
				}
				num = this.ReadCore(this._buffer, 0, this._bufferSize);
				if (num == 0)
				{
					return 0;
				}
				flag = num < this._bufferSize;
				this._readPos = 0;
				this._readLen = num;
			}
			if (num > count)
			{
				num = count;
			}
			Buffer.BlockCopy(this._buffer, this._readPos, array, offset, num);
			this._readPos += num;
			if ((num < count) && !flag)
			{
				int num2 = this.ReadCore(array, offset + num, count - num);
				num += num2;
			}
			return num;
		}

		/// <summary>
		/// Sets the current position of this stream to the given value.
		/// </summary>
		/// <param name="offset">The point relative to <paramref name="origin"/> from which to begin seeking.</param>
		/// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
		/// <returns>The new position within the current stream.</returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			if ((origin < SeekOrigin.Begin) || (origin > SeekOrigin.End))
			{
				throw new ArgumentException();
			}
			if (!this.CanSeek)
			{
				throw new NotSupportedException();
			}
			if (this._writePos > 0)
			{
				this.FlushWrite();
			}
			else if (origin == SeekOrigin.Current)
			{
				offset -= this._readLen - this._readPos;
			}
			this.VerifyOSHandlePosition();
			long num = (long)this._pos + (this._readPos - this._readLen);
			long num2 = this.SeekCore(offset, origin);
			if ((this._appendStart != -1L) && (num2 < this._appendStart))
			{
				this.SeekCore(num, SeekOrigin.Begin);
				throw new IOException();
			}
			if (this._readLen > 0)
			{
				if (num == num2)
				{
					if (this._readPos > 0)
					{
						Buffer.BlockCopy(this._buffer, this._readPos, this._buffer, 0, this._readLen - this._readPos);
						this._readLen -= this._readPos;
						this._readPos = 0;
					}
					if (this._readLen > 0)
					{
						this.SeekCore((long)this._readLen, SeekOrigin.Current);
					}
					return num2;
				}
				if (((num - this._readPos) < num2) && (num2 < ((num + this._readLen) - this._readPos)))
				{
					int num3 = (int)(num2 - num);
					Buffer.BlockCopy(this._buffer, this._readPos + num3, this._buffer, 0, this._readLen - (this._readPos + num3));
					this._readLen -= this._readPos + num3;
					this._readPos = 0;
					if (this._readLen > 0)
					{
						this.SeekCore((long)this._readLen, SeekOrigin.Current);
					}
					return num2;
				}
				this._readPos = 0;
				this._readLen = 0;
			}
			return num2;
		}

		/// <summary>
		/// Sets the length of the current stream.
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		public override void SetLength(long value)
		{
			if (value < 0L)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (!this.CanSeek || !this.CanWrite)
			{
				throw new NotSupportedException();
			}
			if (this._writePos > 0)
			{
				this.FlushWrite();
			}
			else if (this._readPos < this._readLen)
			{
				this.FlushRead();
			}
			if ((this._appendStart != -1L) && (value < this._appendStart))
			{
				throw new IOException();
			}
			this.SetLengthCore(value);
		}

		/// <summary>
		/// Writes a block of bytes to this stream using data from a buffer. 
		/// </summary>
		/// <param name="array">The buffer containing data to write to the stream.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="array"/> at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		public override void Write(byte[] array, int offset, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException();
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if ((array.Length - offset) < count)
			{
				throw new ArgumentException();
			}
			if (this._writePos == 0)
			{
				if (!this.CanWrite)
				{
					throw new NotSupportedException();
				}
				if (this._readPos < this._readLen)
				{
					this.FlushRead();
				}
				this._readPos = 0;
				this._readLen = 0;
			}
			if (this._writePos > 0)
			{
				int num = this._bufferSize - this._writePos;
				if (num > 0)
				{
					if (num > count)
					{
						num = count;
					}
					Buffer.BlockCopy(array, offset, this._buffer, this._writePos, num);
					this._writePos += num;
					if (count == num)
					{
						return;
					}
					offset += num;
					count -= num;
				}
				this.WriteCore(this._buffer, 0, this._writePos);
				this._writePos = 0;
			}
			if (count >= this._bufferSize)
			{
				this.WriteCore(array, offset, count);
			}
			else
			{
				if (this._buffer == null)
				{
					this._buffer = new byte[this._bufferSize];
				}
				Buffer.BlockCopy(array, offset, this._buffer, this._writePos, count);
				this._writePos = count;
			}
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="RemoteFileStream"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (f != null && !f.IsClosed)
				f.Dispose();
		}

		private void FlushRead()
		{
			if ((this._readPos - this._readLen) != 0)
			{
				this.SeekCore((long)(this._readPos - this._readLen), SeekOrigin.Current);
			}
			this._readPos = 0;
			this._readLen = 0;
		}

		private void FlushWrite()
		{
			this.WriteCore(this._buffer, 0, this._writePos);
			this._writePos = 0;
		}

		private int ReadCore(byte[] buffer, int offset, int count)
		{
			int num2 = f.Read(buffer, offset, count);
			if (num2 == -1)
				throw new IOException();
			this._pos += (ulong)num2;
			this.VerifyOSHandlePosition();
			return num2;
		}

		private long SeekCore(long offset, SeekOrigin origin)
		{
			long num = f.Seek(offset, origin);
			this._pos = (ulong)num;
			return num;
		}

		private void SetLengthCore(long value)
		{
			if (value > 0x7fffffffL)
			{
				throw new ArgumentOutOfRangeException();
			}
			ulong offset = this._pos;
			if (this._pos != (ulong)value)
			{
				this.SeekCore(value, SeekOrigin.Begin);
			}
			f.SetEndOfFile();
			if ((long)offset != value)
			{
				if ((long)offset < value)
				{
					this.SeekCore((long)offset, SeekOrigin.Begin);
				}
				else
				{
					this.SeekCore(0L, SeekOrigin.End);
				}
			}
			this.VerifyOSHandlePosition();
		}

		private void VerifyOSHandlePosition()
		{
			if (this.CanSeek)
			{
				ulong num = this._pos;
				if (this.SeekCore(0L, SeekOrigin.Current) != (long)num)
				{
					this._readPos = 0;
					this._readLen = 0;
				}
			}
		}

		private void WriteCore(byte[] buffer, int offset, int count)
		{
			if ((buffer.Length - offset) < count)
				throw new IndexOutOfRangeException();
			int num2 = f.Write(buffer, offset, count);
			if (num2 == -1)
				throw new IOException();
			this._pos += (ulong)num2;
		}
	}
}