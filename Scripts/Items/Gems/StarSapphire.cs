using System;
using Server;

namespace Server.Items
{
	public class StarSapphire : BaseItem
	{
		[Constructable]
		public StarSapphire() : this( 1 )
		{
		}

		[Constructable]
		public StarSapphire( int amount ) : base( 0xF21 )
		{
			Stackable = true;
			Weight = 0.1;
			Amount = amount;
		}

		public StarSapphire( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new StarSapphire( amount ), amount );
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
	}
}