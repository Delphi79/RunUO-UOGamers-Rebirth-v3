using System;
using Server;

namespace Server.Items
{
	public abstract class BaseReagent : BaseItem
	{
		public BaseReagent( int itemID ) : this( itemID, 1 )
		{
		}

		public BaseReagent( int itemID, int amount ) : base( itemID )
		{
			Stackable = true;
			Weight = 0.1;
			Amount = amount;
		}

		public BaseReagent( Serial serial ) : base( serial )
		{
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