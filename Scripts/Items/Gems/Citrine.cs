using System;
using Server;

namespace Server.Items
{
	public class Citrine : BaseItem
	{
		[Constructable]
		public Citrine() : this( 1 )
		{
		}

		[Constructable]
		public Citrine( int amount ) : base( 0xF15 )
		{
			Stackable = true;
			Weight = 0.1;
			Amount = amount;
		}

		public Citrine( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new Citrine( amount ), amount );
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