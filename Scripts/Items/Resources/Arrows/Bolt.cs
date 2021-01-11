using System;

namespace Server.Items
{
	public class Bolt : BaseItem
	{
		[Constructable]
		public Bolt() : this( 1 )
		{
		}

		[Constructable]
		public Bolt( int amount ) : base( 0x1BFB )
		{
			Stackable = true;
			Weight = 0.1;
			Amount = amount;
		}

		public Bolt( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new Bolt( amount ), amount );
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