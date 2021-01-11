using System;
using Server;

namespace Server.Items
{
	public class JackalsCollar : PlateGorget
	{
		public override int LabelNumber{ get{ return 1061594; } } // Jackal's Collar
		public override int ArtifactRarity{ get{ return 11; } }

		public override int InitMinHits{ get{ return 255; } }
		public override int InitMaxHits{ get{ return 255; } }

		[Constructable]
		public JackalsCollar()
		{
			Hue = 0x54B;
			Attributes.BonusDex = 15;
			Attributes.RegenHits = 2;
			FireBonus = 20;
			ColdBonus = 15;
		}

		public JackalsCollar( Serial serial ) : base( serial )
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