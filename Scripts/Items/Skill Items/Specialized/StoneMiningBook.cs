using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Items
{
	public class StoneMiningBook : BaseItem
	{
		[Constructable]
		public StoneMiningBook() : base( 0xFBE )
		{
			Name = "Mining For Quality Stone";
			Weight = 1.0;
		}

		public StoneMiningBook( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}

		public override void OnDoubleClick( Mobile from )
		{
			PlayerMobile pm = from as PlayerMobile;

			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
			else if ( pm == null || from.Skills[SkillName.Mining].Base < 100.0 )
			{
				from.SendAsciiMessage( "Only a Grandmaster Miner can learn from this book." );
			}
			else if ( pm.StoneMining )
			{
				pm.SendAsciiMessage( "You have already learned this knowledge." );
			}
			else
			{
				pm.StoneMining = true;
				pm.SendAsciiMessage( "You have learned to mine for stones. Target mountains when mining to find stones." );
				Delete();
			}
		}
	}
}