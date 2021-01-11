using System;
using Server;

namespace Server.Items
{
	public class LeggingsOfBane : ChainLegs
	{
		public override int LabelNumber{ get{ return 1061100; } } // Leggings of Bane
		public override int ArtifactRarity{ get{ return 11; } }

		public override int InitMinHits{ get{ return 255; } }
		public override int InitMaxHits{ get{ return 255; } }

		[Constructable]
		public LeggingsOfBane()
		{
			Hue = 0x559;
			// TODO: Durability 100% ?
			Attributes.BonusStam = 8;
			Attributes.AttackChance = 20;
			PoisonBonus = 35;
		}

		public LeggingsOfBane( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}