using System;
using Server;

namespace Server.Items
{
	public class TunicOfFire : ChainChest
	{
		public override int LabelNumber{ get{ return 1061099; } } // Tunic of Fire
		public override int ArtifactRarity{ get{ return 11; } }

		public override int InitMinHits{ get{ return 255; } }
		public override int InitMaxHits{ get{ return 255; } }

		[Constructable]
		public TunicOfFire()
		{
			Hue = 0x54E;
			ArmorAttributes.SelfRepair = 5;
			// TODO: Night Sight
			Attributes.ReflectPhysical = 15;
			PhysicalBonus = 20;
			FireBonus = 30;
		}

		public TunicOfFire( Serial serial ) : base( serial )
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