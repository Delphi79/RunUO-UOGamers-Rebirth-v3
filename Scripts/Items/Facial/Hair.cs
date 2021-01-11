using System;

namespace Server.Items
{
	public abstract class Hair : BaseItem
	{
		public Hair( int itemID ) : this( itemID, 0 )
		{
		}

		public Hair( int itemID, int hue ) : base( itemID )
		{
			LootType = LootType.Blessed;
			Layer = Layer.Hair;
			Hue = hue;
		}

		public Hair( Serial serial ) : base( serial )
		{
		}

		public override bool DisplayLootType{ get{ return false; } }

		public override bool VerifyMove( Mobile from )
		{
			return ( from.AccessLevel >= AccessLevel.GameMaster );
		}

		public override DeathMoveResult OnParentDeath( Mobile parent )
		{
			Dupe( Amount );

			return DeathMoveResult.MoveToCorpse;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			LootType = LootType.Blessed;

			int version = reader.ReadInt();
		}
	}

	public class GenericHair : Hair
	{
		[Constructable]
		public GenericHair( int itemID ) : this( itemID, 0 )
		{
		}

		[Constructable]
		public GenericHair( int itemID, int hue ) : base( itemID, hue )
		{
		}

		public GenericHair( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new GenericHair( ItemID, Hue ), amount );
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

	public class Mohawk : Hair
	{
		[Constructable]
		public Mohawk() : this( 0 )
		{
		}

		[Constructable]
		public Mohawk( int hue ) : base( 0x2044, hue )
		{
		}

		public Mohawk( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new Mohawk(), amount );
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

	public class PageboyHair : Hair
	{
		[Constructable]
		public PageboyHair() : this( 0 )
		{
		}

		[Constructable]
		public PageboyHair( int hue ) : base( 0x2045, hue )
		{
		}

		public PageboyHair( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new PageboyHair(), amount );
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

	public class BunsHair : Hair
	{
		[Constructable]
		public BunsHair() : this( 0 )
		{
		}

		[Constructable]
		public BunsHair( int hue ) : base( 0x2046, hue )
		{
		}

		public BunsHair( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new BunsHair(), amount );
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

	public class LongHair : Hair
	{
		[Constructable]
		public LongHair() : this( 0 )
		{
		}

		[Constructable]
		public LongHair( int hue ) : base( 0x203C, hue )
		{
		}

		public LongHair( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new LongHair(), amount );
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

	public class ShortHair : Hair
	{
		[Constructable]
		public ShortHair() : this( 0 )
		{
		}

		[Constructable]
		public ShortHair( int hue ) : base( 0x203B, hue )
		{
		}

		public ShortHair( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new ShortHair(), amount );
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

	public class PonyTail : Hair
	{
		[Constructable]
		public PonyTail() : this( 0 )
		{
		}

		[Constructable]
		public PonyTail( int hue ) : base( 0x203D, hue )
		{
		}

		public PonyTail( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new PonyTail(), amount );
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

	public class Afro : Hair
	{
		[Constructable]
		public Afro() : this( 0 )
		{
		}

		[Constructable]
		public Afro( int hue ) : base( 0x2047, hue )
		{
		}

		public Afro( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new Afro(), amount );
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

	public class ReceedingHair : Hair
	{
		[Constructable]
		public ReceedingHair() : this( 0 )
		{
		}

		[Constructable]
		public ReceedingHair( int hue ) : base( 0x2048, hue )
		{
		}

		public ReceedingHair( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new ReceedingHair(), amount );
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

	public class TwoPigTails : Hair
	{
		[Constructable]
		public TwoPigTails() : this( 0 )
		{
		}

		[Constructable]
		public TwoPigTails( int hue ) : base( 0x2049, hue )
		{
		}

		public TwoPigTails( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new TwoPigTails(), amount );
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

	public class KrisnaHair : Hair
	{
		[Constructable]
		public KrisnaHair() : this( 0 )
		{
		}

		[Constructable]
		public KrisnaHair( int hue ) : base( 0x204A, hue )
		{
		}

		public KrisnaHair( Serial serial ) : base( serial )
		{
		}

		public override Item Dupe( int amount )
		{
			return base.Dupe( new KrisnaHair(), amount );
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