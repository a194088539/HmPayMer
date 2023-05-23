using System;

namespace HM.Framework.Dapper
{
	[Flags]
	public enum CommandFlags
	{
		None = 0x0,
		Buffered = 0x1,
		Pipelined = 0x2,
		NoCache = 0x4
	}
}
