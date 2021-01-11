using System;
using System.Collections; using System.Collections.Generic;
using Server;
using Server.Targeting;

namespace Server.Scripts.Commands
{
	public class SelfCommandImplementor : BaseCommandImplementor
	{
		public SelfCommandImplementor()
		{
			Accessors = new string[]{ "Self" };
			SupportRequirement = CommandSupport.Self;
			AccessLevel = AccessLevel.Counselor;
			Usage = "Self <command>";
			Description = "Invokes the command on the commanding player.";
		}

		public override void Compile( Mobile from, BaseCommand command, ref string[] args, ref object obj )
		{
			if ( command.ObjectTypes == ObjectTypes.Items )
				return; // sanity check

			obj = from;
		}
	}
}