using System;
using Server;
using Server.Mobiles;
using Server.Spells;

namespace Server.Items
{
	public class ClockworkAssembly : BaseItem
	{
		[Constructable]
		public ClockworkAssembly() : base( 0x1EA8 )
		{
			Weight = 5.0;
			Hue = 1102;
			Name = "clockwork assembly";
		}

		public ClockworkAssembly( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
				return;
			}

			double tinkerSkill = from.Skills[SkillName.Tinkering].Value;

			if ( tinkerSkill < 60.0 )
			{
				from.SendAsciiMessage( "You must have at least 60.0 skill in tinkering to construct a golem." );
				return;
			}
			Container pack = from.Backpack;

			if ( pack == null )
				return;

			int res = pack.ConsumeTotal(
				new Type[]
				{
					typeof( PowerCrystal ),
					typeof( IronIngot ),
					typeof( Gears )
				},
				new int[]
				{
					1,
					50,
					50,
					5
				} );

			switch ( res )
			{
				case 0:
				{
					from.SendAsciiMessage( "You must have a power crystal to construct the golem." );
					break;
				}
				case 1:
				{
					from.SendAsciiMessage( "You must have 50 iron ingots to construct the golem." );
					break;
				}
				case 2:
				{
					from.SendAsciiMessage( "You must have 50 bronze ingots to construct the golem." );
					break;
				}
				case 3:
				{
					from.SendAsciiMessage( "You must have 5 gears to construct the golem." );
					break;
				}
				default:
				{
					
					break;
				}
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}