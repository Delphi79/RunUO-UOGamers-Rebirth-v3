using System;
using System.Collections; using System.Collections.Generic;
using Server;
using Server.Targeting;

namespace Server.Scripts.Commands
{
	public class MultiCommandImplementor : BaseCommandImplementor
	{
		public MultiCommandImplementor()
		{
			Accessors = new string[]{ "Multi", "m" };
			SupportRequirement = CommandSupport.Multi;
			AccessLevel = AccessLevel.Counselor;
			Usage = "Multi <command>";
			Description = "Invokes the command on multiple targeted objects.";
		}

		public override void Process( Mobile from, BaseCommand command, string[] args )
		{
			if ( command.ValidateArgs( this, new Server.Commands.CommandEventArgs( from, command.Commands[0], GenerateArgString( args ), args ) ) )
				from.BeginTarget( -1, command.ObjectTypes == ObjectTypes.All, TargetFlags.None, new TargetStateCallback( OnTarget ), new object[]{ command, args } );
		}

		public void OnTarget( Mobile from, object targeted, object state )
		{
			object[] states = (object[])state;
			BaseCommand command = (BaseCommand)states[0];
			string[] args = (string[])states[1];

			if ( !BaseCommand.IsAccessible( from, targeted ) )
			{
				from.SendMessage( "That is not accessible." );
				from.BeginTarget( -1, command.ObjectTypes == ObjectTypes.All, TargetFlags.None, new TargetStateCallback( OnTarget ), new object[]{ command, args } );
				return;
			}

			switch ( command.ObjectTypes )
			{
				case ObjectTypes.Both:
				{
					if ( !(targeted is Item) && !(targeted is Mobile) )
					{
						from.SendMessage( "This command does not work on that." );
						return;
					}

					break;
				}
				case ObjectTypes.Items:
				{
					if ( !(targeted is Item) )
					{
						from.SendMessage( "This command only works on items." );
						return;
					}

					break;
				}
				case ObjectTypes.Mobiles:
				{
					if ( !(targeted is Mobile) )
					{
						from.SendMessage( "This command only works on mobiles." );
						return;
					}

					break;
				}
			}

			RunCommand( from, targeted, command, args );

			from.BeginTarget( -1, command.ObjectTypes == ObjectTypes.All, TargetFlags.None, new TargetStateCallback( OnTarget ), new object[]{ command, args } );
		}
	}
}