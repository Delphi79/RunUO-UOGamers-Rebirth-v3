using System;
using Server;
using Server.Targeting;
using Server.Gumps;
using Server.Scripts.Gumps;

namespace Server.Scripts.Commands
{
	public class Skills
	{
		public static void Initialize()
		{
			Register();
		}

		public static void Register()
		{
			Server.Commands.CommandSystem.Register( "Skills", AccessLevel.Counselor, new Server.Commands.CommandEventHandler( Skills_OnCommand ) );
		}

		private class SkillsTarget : Target
		{
			public SkillsTarget( ) : base( -1, true, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
					from.SendGump( new SkillsGump( from, (Mobile)o ) );
			}
		}

		[Usage( "Skills" )]
		[Description( "Opens a menu where you can view or edit skills of a targeted mobile." )]
		private static void Skills_OnCommand( Server.Commands.CommandEventArgs e )
		{
			e.Mobile.Target = new SkillsTarget();
		}
	}
}