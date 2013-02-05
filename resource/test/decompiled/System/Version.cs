using System;
using System.Globalization;
using System.Runtime.InteropServices;
namespace System
{
	[ComVisible(true)]
	[Serializable]
	public sealed class Version : ICloneable, IComparable, IComparable<Version>, IEquatable<Version>
	{
		internal enum ParseFailureKind
		{
			ArgumentNullException,
			ArgumentException,
			ArgumentOutOfRangeException,
			FormatException
		}
		internal struct VersionResult
		{
			internal Version m_parsedVersion;
			internal Version.ParseFailureKind m_failure;
			internal string m_exceptionArgument;
			internal string m_argumentName;
			internal bool m_canThrow;
			internal void Init(string argumentName, bool canThrow)
			{
				this.m_canThrow = canThrow;
				this.m_argumentName = argumentName;
			}
			internal void SetFailure(Version.ParseFailureKind failure)
			{
				this.SetFailure(failure, string.Empty);
			}
			internal void SetFailure(Version.ParseFailureKind failure, string argument)
			{
				this.m_failure = failure;
				this.m_exceptionArgument = argument;
				if (this.m_canThrow)
				{
					throw this.GetVersionParseException();
				}
			}
			internal Exception GetVersionParseException()
			{
				switch (this.m_failure)
				{
					case Version.ParseFailureKind.ArgumentNullException:
					{
						return new ArgumentNullException(this.m_argumentName);
					}
					case Version.ParseFailureKind.ArgumentException:
					{
						return new ArgumentException(Environment.GetResourceString("Arg_VersionString"));
					}
					case Version.ParseFailureKind.ArgumentOutOfRangeException:
					{
						return new ArgumentOutOfRangeException(this.m_exceptionArgument, Environment.GetResourceString("ArgumentOutOfRange_Version"));
					}
					case Version.ParseFailureKind.FormatException:
					{
						try
						{
							int.Parse(this.m_exceptionArgument, CultureInfo.InvariantCulture);
						}
						catch (FormatException ex)
						{
							Exception result = ex;
							return result;
						}
						catch (OverflowException ex2)
						{
							Exception result = ex2;
							return result;
						}
						return new FormatException(Environment.GetResourceString("Format_InvalidString"));
					}
					default:
					{
						return new ArgumentException(Environment.GetResourceString("Arg_VersionString"));
					}
				}
			}
		}
		private int _Major;
		private int _Minor;
		private int _Build = -1;
		private int _Revision = -1;
		public int Major
		{
			get
			{
				return this._Major;
			}
		}
		public int Minor
		{
			get
			{
				return this._Minor;
			}
		}
		public int Build
		{
			get
			{
				return this._Build;
			}
		}
		public int Revision
		{
			get
			{
				return this._Revision;
			}
		}
		public short MajorRevision
		{
			get
			{
				return (short)(this._Revision >> 16);
			}
		}
		public short MinorRevision
		{
			get
			{
				return (short)(this._Revision & 65535);
			}
		}
		public Version(int major, int minor, int build, int revision)
		{
			if (major < 0)
			{
				throw new ArgumentOutOfRangeException("major", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			if (minor < 0)
			{
				throw new ArgumentOutOfRangeException("minor", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			if (build < 0)
			{
				throw new ArgumentOutOfRangeException("build", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			if (revision < 0)
			{
				throw new ArgumentOutOfRangeException("revision", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			this._Major = major;
			this._Minor = minor;
			this._Build = build;
			this._Revision = revision;
		}
		public Version(int major, int minor, int build)
		{
			if (major < 0)
			{
				throw new ArgumentOutOfRangeException("major", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			if (minor < 0)
			{
				throw new ArgumentOutOfRangeException("minor", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			if (build < 0)
			{
				throw new ArgumentOutOfRangeException("build", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			this._Major = major;
			this._Minor = minor;
			this._Build = build;
		}
		public Version(int major, int minor)
		{
			if (major < 0)
			{
				throw new ArgumentOutOfRangeException("major", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			if (minor < 0)
			{
				throw new ArgumentOutOfRangeException("minor", Environment.GetResourceString("ArgumentOutOfRange_Version"));
			}
			this._Major = major;
			this._Minor = minor;
		}
		public Version(string version)
		{
			Version version2 = Version.Parse(version);
			this._Major = version2.Major;
			this._Minor = version2.Minor;
			this._Build = version2.Build;
			this._Revision = version2.Revision;
		}
		public Version()
		{
			this._Major = 0;
			this._Minor = 0;
		}
		public object Clone()
		{
			return new Version
			{
				_Major = this._Major, 
				_Minor = this._Minor, 
				_Build = this._Build, 
				_Revision = this._Revision
			};
		}
		public int CompareTo(object version)
		{
			if (version == null)
			{
				return 1;
			}
			Version version2 = version as Version;
			if (version2 == null)
			{
				throw new ArgumentException(Environment.GetResourceString("Arg_MustBeVersion"));
			}
			if (this._Major != version2._Major)
			{
				if (this._Major > version2._Major)
				{
					return 1;
				}
				return -1;
			}
			else
			{
				if (this._Minor != version2._Minor)
				{
					if (this._Minor > version2._Minor)
					{
						return 1;
					}
					return -1;
				}
				else
				{
					if (this._Build != version2._Build)
					{
						if (this._Build > version2._Build)
						{
							return 1;
						}
						return -1;
					}
					else
					{
						if (this._Revision == version2._Revision)
						{
							return 0;
						}
						if (this._Revision > version2._Revision)
						{
							return 1;
						}
						return -1;
					}
				}
			}
		}
		public int CompareTo(Version value)
		{
			if (value == null)
			{
				return 1;
			}
			if (this._Major != value._Major)
			{
				if (this._Major > value._Major)
				{
					return 1;
				}
				return -1;
			}
			else
			{
				if (this._Minor != value._Minor)
				{
					if (this._Minor > value._Minor)
					{
						return 1;
					}
					return -1;
				}
				else
				{
					if (this._Build != value._Build)
					{
						if (this._Build > value._Build)
						{
							return 1;
						}
						return -1;
					}
					else
					{
						if (this._Revision == value._Revision)
						{
							return 0;
						}
						if (this._Revision > value._Revision)
						{
							return 1;
						}
						return -1;
					}
				}
			}
		}
		public override bool Equals(object obj)
		{
			Version version = obj as Version;
			return !(version == null) && this._Major == version._Major && this._Minor == version._Minor && this._Build == version._Build && this._Revision == version._Revision;
		}
		public bool Equals(Version obj)
		{
			return !(obj == null) && this._Major == obj._Major && this._Minor == obj._Minor && this._Build == obj._Build && this._Revision == obj._Revision;
		}
		public override int GetHashCode()
		{
			int num = 0;
			num |= (this._Major & 15) << 28;
			num |= (this._Minor & 255) << 20;
			num |= (this._Build & 255) << 12;
			return num | (this._Revision & 4095);
		}
		public override string ToString()
		{
			if (this._Build == -1)
			{
				return this.ToString(2);
			}
			if (this._Revision == -1)
			{
				return this.ToString(3);
			}
			return this.ToString(4);
		}
		public string ToString(int fieldCount)
		{
			switch (fieldCount)
			{
				case 0:
				{
					return string.Empty;
				}
				case 1:
				{
					return string.Concat(this._Major);
				}
				case 2:
				{
					return this._Major + "." + this._Minor;
				}
				default:
				{
					if (this._Build == -1)
					{
						throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper", new object[]
						{
							"0", 
							"2"
						}), "fieldCount");
					}
					if (fieldCount == 3)
					{
						return string.Concat(new object[]
						{
							this._Major, 
							".", 
							this._Minor, 
							".", 
							this._Build
						});
					}
					if (this._Revision == -1)
					{
						throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper", new object[]
						{
							"0", 
							"3"
						}), "fieldCount");
					}
					if (fieldCount == 4)
					{
						return string.Concat(new object[]
						{
							this.Major, 
							".", 
							this._Minor, 
							".", 
							this._Build, 
							".", 
							this._Revision
						});
					}
					throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper", new object[]
					{
						"0", 
						"4"
					}), "fieldCount");
				}
			}
		}
		public static Version Parse(string input)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			Version.VersionResult versionResult = default(Version.VersionResult);
			versionResult.Init("input", true);
			if (!Version.TryParseVersion(input, ref versionResult))
			{
				throw versionResult.GetVersionParseException();
			}
			return versionResult.m_parsedVersion;
		}
		public static bool TryParse(string input, out Version result)
		{
			Version.VersionResult versionResult = default(Version.VersionResult);
			versionResult.Init("input", false);
			bool result2 = Version.TryParseVersion(input, ref versionResult);
			result = versionResult.m_parsedVersion;
			return result2;
		}
		private static bool TryParseVersion(string version, ref Version.VersionResult result)
		{
			if (version == null)
			{
				result.SetFailure(Version.ParseFailureKind.ArgumentNullException);
				return false;
			}
			string[] array = version.Split(new char[]
			{
				'.'
			});
			int num = array.Length;
			if (num < 2 || num > 4)
			{
				result.SetFailure(Version.ParseFailureKind.ArgumentException);
				return false;
			}
			int major;
			if (!Version.TryParseComponent(array[0], "version", ref result, out major))
			{
				return false;
			}
			int minor;
			if (!Version.TryParseComponent(array[1], "version", ref result, out minor))
			{
				return false;
			}
			num -= 2;
			if (num > 0)
			{
				int build;
				if (!Version.TryParseComponent(array[2], "build", ref result, out build))
				{
					return false;
				}
				num--;
				if (num > 0)
				{
					int revision;
					if (!Version.TryParseComponent(array[3], "revision", ref result, out revision))
					{
						return false;
					}
					result.m_parsedVersion = new Version(major, minor, build, revision);
				}
				else
				{
					result.m_parsedVersion = new Version(major, minor, build);
				}
			}
			else
			{
				result.m_parsedVersion = new Version(major, minor);
			}
			return true;
		}
		private static bool TryParseComponent(string component, string componentName, ref Version.VersionResult result, out int parsedComponent)
		{
			if (!int.TryParse(component, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedComponent))
			{
				result.SetFailure(Version.ParseFailureKind.FormatException, component);
				return false;
			}
			if (parsedComponent < 0)
			{
				result.SetFailure(Version.ParseFailureKind.ArgumentOutOfRangeException, componentName);
				return false;
			}
			return true;
		}
		public static bool operator ==(Version v1, Version v2)
		{
			if (object.ReferenceEquals(v1, null))
			{
				return object.ReferenceEquals(v2, null);
			}
			return v1.Equals(v2);
		}
		public static bool operator !=(Version v1, Version v2)
		{
			return !(v1 == v2);
		}
		public static bool operator <(Version v1, Version v2)
		{
			if (v1 == null)
			{
				throw new ArgumentNullException("v1");
			}
			return v1.CompareTo(v2) < 0;
		}
		public static bool operator <=(Version v1, Version v2)
		{
			if (v1 == null)
			{
				throw new ArgumentNullException("v1");
			}
			return v1.CompareTo(v2) <= 0;
		}
		public static bool operator >(Version v1, Version v2)
		{
			return v2 < v1;
		}
		public static bool operator >=(Version v1, Version v2)
		{
			return v2 <= v1;
		}
	}
}
