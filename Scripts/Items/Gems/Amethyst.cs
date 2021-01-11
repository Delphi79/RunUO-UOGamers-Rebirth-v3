using System;
using Server;

namespace Server.Items
{
	public class Amethyst : BaseItem
	{
		[Constructable]
		public Amethyst() : this( 1 )
		{
		}

		[Constructable]
		public Amethyst( int amount ) : base( 0xF16 )
		{
			Stackable = true;
			Weight = 0.1;
			Amount = amount;
		}

		public Amethyst( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new Amethyst( amount ), amount );
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