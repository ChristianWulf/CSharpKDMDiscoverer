using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using System.Text;
namespace System
{
	internal static class DateTimeFormat
	{
		internal const int MaxSecondsFractionDigits = 7;
		internal const string RoundtripFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK";
		internal const string RoundtripDateTimeUnfixed = "yyyy'-'MM'-'ddTHH':'mm':'ss zzz";
		private const int DEFAULT_ALL_DATETIMES_SIZE = 132;
		internal static readonly TimeSpan NullOffset = TimeSpan.MinValue;
		internal static char[] allStandardFormats = new char[]
		{
			'd', 
			'D', 
			'f', 
			'F', 
			'g', 
			'G', 
			'm', 
			'M', 
			'o', 
			'O', 
			'r', 
			'R', 
			's', 
			't', 
			'T', 
			'u', 
			'U', 
			'y', 
			'Y'
		};
		internal static string[] fixedNumberFormats = new string[]
		{
			"0", 
			"00", 
			"000", 
			"0000", 
			"00000", 
			"000000", 
			"0000000"
		};
		[SecuritySafeCritical]
		internal static void FormatDigits(StringBuilder outputBuffer, int value, int len)
		{
			DateTimeFormat.FormatDigits(outputBuffer, value, len, false);
		}
		[SecuritySafeCritical]
		internal unsafe static void FormatDigits(StringBuilder outputBuffer, int value, int len, bool overrideLengthLimit)
		{
			if (!overrideLengthLimit && len > 2)
			{
				len = 2;
			}
			char* ptr = stackalloc char[(UIntPtr)16];
			char* ptr2 = ptr + (IntPtr)32 / 2;
			int num = value;
			do
			{
				*(ptr2 -= (IntPtr)2 / 2) = (char)(num % 10 + 48);
				num /= 10;
			}
			while (num != 0 && ptr2 != ptr);
			int num2 = (ptr + (IntPtr)32 / 2 - ptr2 / 2) / 2;
			while (num2 < len && ptr2 != ptr)
			{
				*(ptr2 -= (IntPtr)2 / 2) = '0';
				num2++;
			}
			outputBuffer.Append(ptr2, num2);
		}
		private static void HebrewFormatDigits(StringBuilder outputBuffer, int digits)
		{
			outputBuffer.Append(HebrewNumber.ToString(digits));
		}
		internal static int ParseRepeatPattern(string format, int pos, char patternChar)
		{
			int length = format.Length;
			int num = pos + 1;
			while (num < length && format[num] == patternChar)
			{
				num++;
			}
			return num - pos;
		}
		private static string FormatDayOfWeek(int dayOfWeek, int repeat, DateTimeFormatInfo dtfi)
		{
			if (repeat == 3)
			{
				return dtfi.GetAbbreviatedDayName((DayOfWeek)dayOfWeek);
			}
			return dtfi.GetDayName((DayOfWeek)dayOfWeek);
		}
		private static string FormatMonth(int month, int repeatCount, DateTimeFormatInfo dtfi)
		{
			if (repeatCount == 3)
			{
				return dtfi.GetAbbreviatedMonthName(month);
			}
			return dtfi.GetMonthName(month);
		}
		private static string FormatHebrewMonthName(DateTime time, int month, int repeatCount, DateTimeFormatInfo dtfi)
		{
			if (dtfi.Calendar.IsLeapYear(dtfi.Calendar.GetYear(time)))
			{
				return dtfi.internalGetMonthName(month, MonthNameStyles.LeapYear, repeatCount == 3);
			}
			if (month >= 7)
			{
				month++;
			}
			if (repeatCount == 3)
			{
				return dtfi.GetAbbreviatedMonthName(month);
			}
			return dtfi.GetMonthName(month);
		}
		internal static int ParseQuoteString(string format, int pos, StringBuilder result)
		{
			int length = format.Length;
			int num = pos;
			char c = format[pos++];
			bool flag = false;
			while (pos < length)
			{
				char c2 = format[pos++];
				if (c2 == c)
				{
					flag = true;
					break;
				}
				if (c2 == '\\')
				{
					if (pos >= length)
					{
						throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
					}
					result.Append(format[pos++]);
				}
				else
				{
					result.Append(c2);
				}
			}
			if (!flag)
			{
				throw new FormatException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Format_BadQuote"), new object[]
				{
					c
				}));
			}
			return pos - num;
		}
		internal static int ParseNextChar(string format, int pos)
		{
			if (pos >= format.Length - 1)
			{
				return -1;
			}
			return (int)format[pos + 1];
		}
		private static bool IsUseGenitiveForm(string format, int index, int tokenLen, char patternToMatch)
		{
			int num = 0;
			int num2 = index - 1;
			while (num2 >= 0 && format[num2] != patternToMatch)
			{
				num2--;
			}
			if (num2 >= 0)
			{
				while (--num2 >= 0 && format[num2] == patternToMatch)
				{
					num++;
				}
				if (num <= 1)
				{
					return true;
				}
			}
			num2 = index + tokenLen;
			while (num2 < format.Length && format[num2] != patternToMatch)
			{
				num2++;
			}
			if (num2 < format.Length)
			{
				num = 0;
				while (++num2 < format.Length && format[num2] == patternToMatch)
				{
					num++;
				}
				if (num <= 1)
				{
					return true;
				}
			}
			return false;
		}
		private static string FormatCustomized(DateTime dateTime, string format, DateTimeFormatInfo dtfi, TimeSpan offset)
		{
			Calendar calendar = dtfi.Calendar;
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = calendar.ID == 8;
			bool timeOnly = true;
			int i = 0;
			while (i < format.Length)
			{
				char c = format[i];
				char c2 = c;
				int num2;
				if (c2 <= 'H')
				{
					if (c2 <= '\'')
					{
						if (c2 != '"')
						{
							switch (c2)
							{
								case '%':
								{
									int num = DateTimeFormat.ParseNextChar(format, i);
									if (num >= 0 && num != 37)
									{
										stringBuilder.Append(DateTimeFormat.FormatCustomized(dateTime, ((char)num).ToString(), dtfi, offset));
										num2 = 2;
										goto IL_5B0;
									}
									throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
								}
								case '&':
								{
									goto IL_5A4;
								}
								case '\'':
								{
									break;
								}
								default:
								{
									goto IL_5A4;
								}
							}
						}
						StringBuilder stringBuilder2 = new StringBuilder();
						num2 = DateTimeFormat.ParseQuoteString(format, i, stringBuilder2);
						stringBuilder.Append(stringBuilder2);
					}
					else
					{
						if (c2 != '/')
						{
							if (c2 != ':')
							{
								switch (c2)
								{
									case 'F':
									{
										goto IL_1BA;
									}
									case 'G':
									{
										goto IL_5A4;
									}
									case 'H':
									{
										num2 = DateTimeFormat.ParseRepeatPattern(format, i, c);
										DateTimeFormat.FormatDigits(stringBuilder, dateTime.Hour, num2);
										break;
									}
									default:
									{
										goto IL_5A4;
									}
								}
							}
							else
							{
								stringBuilder.Append(dtfi.TimeSeparator);
								num2 = 1;
							}
						}
						else
						{
							stringBuilder.Append(dtfi.DateSeparator);
							num2 = 1;
						}
					}
				}
				else
				{
					if (c2 <= 'h')
					{
						switch (c2)
						{
							case 'K':
							{
								num2 = 1;
								DateTimeFormat.FormatCustomizedRoundripTimeZone(dateTime, offset, stringBuilder);
								break;
							}
							case 'L':
							{
								goto IL_5A4;
							}
							case 'M':
							{
								num2 = DateTimeFormat.ParseRepeatPattern(format, i, c);
								int month = calendar.GetMonth(dateTime);
								if (num2 <= 2)
								{
									if (flag)
									{
										DateTimeFormat.HebrewFormatDigits(stringBuilder, month);
									}
									else
									{
										DateTimeFormat.FormatDigits(stringBuilder, month, num2);
									}
								}
								else
								{
									if (flag)
									{
										stringBuilder.Append(DateTimeFormat.FormatHebrewMonthName(dateTime, month, num2, dtfi));
									}
									else
									{
										if ((dtfi.FormatFlags & DateTimeFormatFlags.UseGenitiveMonth) != DateTimeFormatFlags.None && num2 >= 4)
										{
											stringBuilder.Append(dtfi.internalGetMonthName(month, DateTimeFormat.IsUseGenitiveForm(format, i, num2, 'd') ? MonthNameStyles.Genitive : MonthNameStyles.Regular, false));
										}
										else
										{
											stringBuilder.Append(DateTimeFormat.FormatMonth(month, num2, dtfi));
										}
									}
								}
								timeOnly = false;
								break;
							}
							default:
							{
								if (c2 != '\\')
								{
									switch (c2)
									{
										case 'd':
										{
											num2 = DateTimeFormat.ParseRepeatPattern(format, i, c);
											if (num2 <= 2)
											{
												int dayOfMonth = calendar.GetDayOfMonth(dateTime);
												if (flag)
												{
													DateTimeFormat.HebrewFormatDigits(stringBuilder, dayOfMonth);
												}
												else
												{
													DateTimeFormat.FormatDigits(stringBuilder, dayOfMonth, num2);
												}
											}
											else
											{
												int dayOfWeek = (int)calendar.GetDayOfWeek(dateTime);
												stringBuilder.Append(DateTimeFormat.FormatDayOfWeek(dayOfWeek, num2, dtfi));
											}
											timeOnly = false;
											break;
										}
										case 'e':
										{
											goto IL_5A4;
										}
										case 'f':
										{
											goto IL_1BA;
										}
										case 'g':
										{
											num2 = DateTimeFormat.ParseRepeatPattern(format, i, c);
											stringBuilder.Append(dtfi.GetEraName(calendar.GetEra(dateTime)));
											break;
										}
										case 'h':
										{
											num2 = DateTimeFormat.ParseRepeatPattern(format, i, c);
											int num3 = dateTime.Hour % 12;
											if (num3 == 0)
											{
												num3 = 12;
											}
											DateTimeFormat.FormatDigits(stringBuilder, num3, num2);
											break;
										}
										default:
										{
											goto IL_5A4;
										}
									}
								}
								else
								{
									int num = DateTimeFormat.ParseNextChar(format, i);
									if (num < 0)
									{
										throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
									}
									stringBuilder.Append((char)num);
									num2 = 2;
								}
								break;
							}
						}
					}
					else
					{
						if (c2 != 'm')
						{
							switch (c2)
							{
								case 's':
								{
									num2 = DateTimeFormat.ParseRepeatPattern(format, i, c);
									DateTimeFormat.FormatDigits(stringBuilder, dateTime.Second, num2);
									break;
								}
								case 't':
								{
									num2 = DateTimeFormat.ParseRepeatPattern(format, i, c);
									if (num2 == 1)
									{
										if (dateTime.Hour < 12)
										{
											if (dtfi.AMDesignator.Length >= 1)
											{
												stringBuilder.Append(dtfi.AMDesignator[0]);
											}
										}
										else
										{
											if (dtfi.PMDesignator.Length >= 1)
											{
												stringBuilder.Append(dtfi.PMDesignator[0]);
											}
										}
									}
									else
									{
										stringBuilder.Append((dateTime.Hour < 12) ? dtfi.AMDesignator : dtfi.PMDesignator);
									}
									break;
								}
								default:
								{
									switch (c2)
									{
										case 'y':
										{
											int year = calendar.GetYear(dateTime);
											num2 = DateTimeFormat.ParseRepeatPattern(format, i, c);
											if (dtfi.HasForceTwoDigitYears)
											{
												DateTimeFormat.FormatDigits(stringBuilder, year, (num2 <= 2) ? num2 : 2);
											}
											else
											{
												if (calendar.ID == 8)
												{
													DateTimeFormat.HebrewFormatDigits(stringBuilder, year);
												}
												else
												{
													if (num2 <= 2)
													{
														DateTimeFormat.FormatDigits(stringBuilder, year % 100, num2);
													}
													else
													{
														string format2 = "D" + num2;
														stringBuilder.Append(year.ToString(format2, CultureInfo.InvariantCulture));
													}
												}
											}
											timeOnly = false;
											break;
										}
										case 'z':
										{
											num2 = DateTimeFormat.ParseRepeatPattern(format, i, c);
											DateTimeFormat.FormatCustomizedTimeZone(dateTime, offset, format, num2, timeOnly, stringBuilder);
											break;
										}
										default:
										{
											goto IL_5A4;
										}
									}
									break;
								}
							}
						}
						else
						{
							num2 = DateTimeFormat.ParseRepeatPattern(format, i, c);
							DateTimeFormat.FormatDigits(stringBuilder, dateTime.Minute, num2);
						}
					}
				}
				IL_5B0:
				i += num2;
				continue;
				IL_1BA:
				num2 = DateTimeFormat.ParseRepeatPattern(format, i, c);
				if (num2 > 7)
				{
					throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
				}
				long num4 = dateTime.Ticks % 10000000L;
				num4 /= (long)Math.Pow(10.0, (double)(7 - num2));
				if (c == 'f')
				{
					stringBuilder.Append(((int)num4).ToString(DateTimeFormat.fixedNumberFormats[num2 - 1], CultureInfo.InvariantCulture));
					goto IL_5B0;
				}
				int num5 = num2;
				while (num5 > 0 && num4 % 10L == 0L)
				{
					num4 /= 10L;
					num5--;
				}
				if (num5 > 0)
				{
					stringBuilder.Append(((int)num4).ToString(DateTimeFormat.fixedNumberFormats[num5 - 1], CultureInfo.InvariantCulture));
					goto IL_5B0;
				}
				if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] == '.')
				{
					stringBuilder.Remove(stringBuilder.Length - 1, 1);
					goto IL_5B0;
				}
				goto IL_5B0;
				IL_5A4:
				stringBuilder.Append(c);
				num2 = 1;
				goto IL_5B0;
			}
			return stringBuilder.ToString();
		}
		private static void FormatCustomizedTimeZone(DateTime dateTime, TimeSpan offset, string format, int tokenLen, bool timeOnly, StringBuilder result)
		{
			bool flag = offset == DateTimeFormat.NullOffset;
			if (flag)
			{
				if (timeOnly && dateTime.Ticks < 864000000000L)
				{
					offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now, TimeZoneInfoOptions.NoThrowOnInvalidTime);
				}
				else
				{
					if (dateTime.Kind == DateTimeKind.Utc)
					{
						DateTimeFormat.InvalidFormatForUtc(format, dateTime);
						dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
						offset = TimeZoneInfo.Local.GetUtcOffset(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime);
					}
					else
					{
						offset = TimeZoneInfo.Local.GetUtcOffset(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime);
					}
				}
			}
			if (offset >= TimeSpan.Zero)
			{
				result.Append('+');
			}
			else
			{
				result.Append('-');
				offset = offset.Negate();
			}
			if (tokenLen <= 1)
			{
				result.AppendFormat(CultureInfo.InvariantCulture, "{0:0}", new object[]
				{
					offset.Hours
				});
				return;
			}
			result.AppendFormat(CultureInfo.InvariantCulture, "{0:00}", new object[]
			{
				offset.Hours
			});
			if (tokenLen >= 3)
			{
				result.AppendFormat(CultureInfo.InvariantCulture, ":{0:00}", new object[]
				{
					offset.Minutes
				});
			}
		}
		private static void FormatCustomizedRoundripTimeZone(DateTime dateTime, TimeSpan offset, StringBuilder result)
		{
			if (offset == DateTimeFormat.NullOffset)
			{
				switch (dateTime.Kind)
				{
					case DateTimeKind.Utc:
					{
						result.Append("Z");
						return;
					}
					case DateTimeKind.Local:
					{
						offset = TimeZoneInfo.Local.GetUtcOffset(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime);
						break;
					}
					default:
					{
						return;
					}
				}
			}
			if (offset >= TimeSpan.Zero)
			{
				result.Append('+');
			}
			else
			{
				result.Append('-');
				offset = offset.Negate();
			}
			result.AppendFormat(CultureInfo.InvariantCulture, "{0:00}:{1:00}", new object[]
			{
				offset.Hours, 
				offset.Minutes
			});
		}
		internal static string GetRealFormat(string format, DateTimeFormatInfo dtfi)
		{
			string result = null;
			char c = format[0];
			if (c > 'U')
			{
				if (c != 'Y')
				{
					switch (c)
					{
						case 'd':
						{
							result = dtfi.ShortDatePattern;
							return result;
						}
						case 'e':
						{
							goto IL_159;
						}
						case 'f':
						{
							result = dtfi.LongDatePattern + " " + dtfi.ShortTimePattern;
							return result;
						}
						case 'g':
						{
							result = dtfi.GeneralShortTimePattern;
							return result;
						}
						default:
						{
							switch (c)
							{
								case 'm':
								{
									goto IL_109;
								}
								case 'n':
								case 'p':
								case 'q':
								case 'v':
								case 'w':
								case 'x':
								{
									goto IL_159;
								}
								case 'o':
								{
									goto IL_112;
								}
								case 'r':
								{
									goto IL_11A;
								}
								case 's':
								{
									result = dtfi.SortableDateTimePattern;
									return result;
								}
								case 't':
								{
									result = dtfi.ShortTimePattern;
									return result;
								}
								case 'u':
								{
									result = dtfi.UniversalSortableDateTimePattern;
									return result;
								}
								case 'y':
								{
									break;
								}
								default:
								{
									goto IL_159;
								}
							}
							break;
						}
					}
				}
				result = dtfi.YearMonthPattern;
				return result;
			}
			switch (c)
			{
				case 'D':
				{
					result = dtfi.LongDatePattern;
					return result;
				}
				case 'E':
				{
					goto IL_159;
				}
				case 'F':
				{
					result = dtfi.FullDateTimePattern;
					return result;
				}
				case 'G':
				{
					result = dtfi.GeneralLongTimePattern;
					return result;
				}
				default:
				{
					switch (c)
					{
						case 'M':
						{
							break;
						}
						case 'N':
						case 'P':
						case 'Q':
						case 'S':
						{
							goto IL_159;
						}
						case 'O':
						{
							goto IL_112;
						}
						case 'R':
						{
							goto IL_11A;
						}
						case 'T':
						{
							result = dtfi.LongTimePattern;
							return result;
						}
						case 'U':
						{
							result = dtfi.FullDateTimePattern;
							return result;
						}
						default:
						{
							goto IL_159;
						}
					}
					break;
				}
			}
			IL_109:
			result = dtfi.MonthDayPattern;
			return result;
			IL_112:
			result = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK";
			return result;
			IL_11A:
			result = dtfi.RFC1123Pattern;
			return result;
			IL_159:
			throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
		}
		private static string ExpandPredefinedFormat(string format, ref DateTime dateTime, ref DateTimeFormatInfo dtfi, ref TimeSpan offset)
		{
			char c = format[0];
			if (c <= 'R')
			{
				if (c != 'O')
				{
					if (c != 'R')
					{
						goto IL_160;
					}
					goto IL_5A;
				}
			}
			else
			{
				if (c != 'U')
				{
					switch (c)
					{
						case 'o':
						{
							break;
						}
						case 'p':
						case 'q':
						case 't':
						{
							goto IL_160;
						}
						case 'r':
						{
							goto IL_5A;
						}
						case 's':
						{
							dtfi = DateTimeFormatInfo.InvariantInfo;
							goto IL_160;
						}
						case 'u':
						{
							if (offset != DateTimeFormat.NullOffset)
							{
								dateTime -= offset;
							}
							else
							{
								if (dateTime.Kind == DateTimeKind.Local)
								{
									DateTimeFormat.InvalidFormatForLocal(format, dateTime);
								}
							}
							dtfi = DateTimeFormatInfo.InvariantInfo;
							goto IL_160;
						}
						default:
						{
							goto IL_160;
						}
					}
				}
				else
				{
					if (offset != DateTimeFormat.NullOffset)
					{
						throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
					}
					dtfi = (DateTimeFormatInfo)dtfi.Clone();
					if (dtfi.Calendar.GetType() != typeof(GregorianCalendar))
					{
						dtfi.Calendar = GregorianCalendar.GetDefaultInstance();
					}
					dateTime = dateTime.ToUniversalTime();
					goto IL_160;
				}
			}
			dtfi = DateTimeFormatInfo.InvariantInfo;
			goto IL_160;
			IL_5A:
			if (offset != DateTimeFormat.NullOffset)
			{
				dateTime -= offset;
			}
			else
			{
				if (dateTime.Kind == DateTimeKind.Local)
				{
					DateTimeFormat.InvalidFormatForLocal(format, dateTime);
				}
			}
			dtfi = DateTimeFormatInfo.InvariantInfo;
			IL_160:
			format = DateTimeFormat.GetRealFormat(format, dtfi);
			return format;
		}
		internal static string Format(DateTime dateTime, string format, DateTimeFormatInfo dtfi)
		{
			return DateTimeFormat.Format(dateTime, format, dtfi, DateTimeFormat.NullOffset);
		}
		internal static string Format(DateTime dateTime, string format, DateTimeFormatInfo dtfi, TimeSpan offset)
		{
			if (format == null || format.Length == 0)
			{
				bool flag = false;
				if (dateTime.Ticks < 864000000000L)
				{
					int iD = dtfi.Calendar.ID;
					switch (iD)
					{
						case 3:
						case 4:
						case 6:
						case 8:
						{
							break;
						}
						case 5:
						case 7:
						{
							goto IL_61;
						}
						default:
						{
							if (iD != 13 && iD != 23)
							{
								goto IL_61;
							}
							break;
						}
					}
					flag = true;
					dtfi = DateTimeFormatInfo.InvariantInfo;
				}
				IL_61:
				if (offset == DateTimeFormat.NullOffset)
				{
					if (flag)
					{
						format = "s";
					}
					else
					{
						format = "G";
					}
				}
				else
				{
					if (flag)
					{
						format = "yyyy'-'MM'-'ddTHH':'mm':'ss zzz";
					}
					else
					{
						format = dtfi.DateTimeOffsetPattern;
					}
				}
			}
			if (format.Length == 1)
			{
				format = DateTimeFormat.ExpandPredefinedFormat(format, ref dateTime, ref dtfi, ref offset);
			}
			return DateTimeFormat.FormatCustomized(dateTime, format, dtfi, offset);
		}
		internal static string[] GetAllDateTimes(DateTime dateTime, char format, DateTimeFormatInfo dtfi)
		{
			string[] array = null;
			string[] allDateTimePatterns;
			if (format <= 'U')
			{
				switch (format)
				{
					case 'D':
					case 'F':
					case 'G':
					{
						break;
					}
					case 'E':
					{
						goto IL_153;
					}
					default:
					{
						switch (format)
						{
							case 'M':
							case 'T':
							{
								break;
							}
							case 'N':
							case 'P':
							case 'Q':
							case 'S':
							{
								goto IL_153;
							}
							case 'O':
							case 'R':
							{
								goto IL_127;
							}
							case 'U':
							{
								DateTime dateTime2 = dateTime.ToUniversalTime();
								allDateTimePatterns = dtfi.GetAllDateTimePatterns(format);
								array = new string[allDateTimePatterns.Length];
								for (int i = 0; i < allDateTimePatterns.Length; i++)
								{
									array[i] = DateTimeFormat.Format(dateTime2, allDateTimePatterns[i], dtfi);
								}
								return array;
							}
							default:
							{
								goto IL_153;
							}
						}
						break;
					}
				}
			}
			else
			{
				if (format != 'Y')
				{
					switch (format)
					{
						case 'd':
						case 'f':
						case 'g':
						{
							break;
						}
						case 'e':
						{
							goto IL_153;
						}
						default:
						{
							switch (format)
							{
								case 'm':
								case 't':
								case 'y':
								{
									break;
								}
								case 'n':
								case 'p':
								case 'q':
								case 'v':
								case 'w':
								case 'x':
								{
									goto IL_153;
								}
								case 'o':
								case 'r':
								case 's':
								case 'u':
								{
									goto IL_127;
								}
								default:
								{
									goto IL_153;
								}
							}
							break;
						}
					}
				}
			}
			allDateTimePatterns = dtfi.GetAllDateTimePatterns(format);
			array = new string[allDateTimePatterns.Length];
			for (int j = 0; j < allDateTimePatterns.Length; j++)
			{
				array[j] = DateTimeFormat.Format(dateTime, allDateTimePatterns[j], dtfi);
			}
			return array;
			IL_127:
			array = new string[]
			{
				DateTimeFormat.Format(dateTime, new string(new char[]
				{
					format
				}), dtfi)
			};
			return array;
			IL_153:
			throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
		}
		internal static string[] GetAllDateTimes(DateTime dateTime, DateTimeFormatInfo dtfi)
		{
			List<string> list = new List<string>(132);
			for (int i = 0; i < DateTimeFormat.allStandardFormats.Length; i++)
			{
				string[] allDateTimes = DateTimeFormat.GetAllDateTimes(dateTime, DateTimeFormat.allStandardFormats[i], dtfi);
				for (int j = 0; j < allDateTimes.Length; j++)
				{
					list.Add(allDateTimes[j]);
				}
			}
			string[] array = new string[list.Count];
			list.CopyTo(0, array, 0, list.Count);
			return array;
		}
		internal static void InvalidFormatForLocal(string format, DateTime dateTime)
		{
		}
		[SecuritySafeCritical]
		internal static void InvalidFormatForUtc(string format, DateTime dateTime)
		{
			Mda.DateTimeInvalidLocalFormat();
		}
	}
}
