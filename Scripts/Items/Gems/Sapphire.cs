using System;
using Server;

namespace Server.Items
{
	public class Sapphire : BaseItem
	{
		[Constructable]
		public Sapphire() : this( 1 )
		{
		}

		[Constructable]
		public Sapphire( int amount ) : base( 0xF19 )
		{
			Stackable = true;
			Weight = 0.1;
			Amount = amount;
		}

		public Sapphire( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new Sapphire( amount ), amount );
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