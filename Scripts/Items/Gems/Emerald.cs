using System;
using Server;

namespace Server.Items
{
	public class Emerald : BaseItem
	{
		[Constructable]
		public Emerald() : this( 1 )
		{
		}

		[Constructable]
		public Emerald( int amount ) : base( 0xF10 )
		{
			Stackable = true;
			Weight = 0.1;
			Amount = amount;
		}

		public Emerald( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new Emerald( amount ), amount );
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