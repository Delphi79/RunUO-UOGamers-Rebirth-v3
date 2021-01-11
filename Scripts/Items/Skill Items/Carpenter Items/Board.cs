using System;

namespace Server.Items
{
	[FlipableAttribute(0x1BD7, 0x1BDA)]
	public class Board : BaseItem, ICommodity, ICarvable, IChopable
	{
		string ICommodity.Description
		{
			get
			{
				return String.Format( Amount == 1 ? "{0} board" : "{0} boards", Amount );
			}
		}

		[Constructable]
		public Board() : this(1)
		{
		}

		[Constructable]
		public Board(int amount) : base(0x1BD7)
		{
			Stackable = true;
			Weight = 0.1;
			Amount = amount;
		}

		public Board(Serial serial) : base(serial)
		{
		}

		public override Item Dupe(int amount)
		{
			return base.Dupe(new Board(amount), amount);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public void Carve( Mobile from, Item blade )
		{
			OnChop( from );
		}
		
		public void OnChop(Mobile from)
		{
			if ( this.IsChildOf( from.Backpack ) )
				new Engines.Craft.BowcraftSystem( this ).Begin( from, null );
			else
				from.SendAsciiMessage( "That belongs to someone else." );
		}
	}
}

