using System;

namespace Server.Items
{
	public class BlankScroll : BaseItem
	{
		[Constructable]
		public BlankScroll() : this( 1 )
		{
		}

		[Constructable]
		public BlankScroll( int amount ) : base( 0xEF3 )
		{
			Stackable = true;
			Weight = 1.0;
			Amount = amount;
		}

		public BlankScroll( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new BlankScroll( amount ), amount );
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