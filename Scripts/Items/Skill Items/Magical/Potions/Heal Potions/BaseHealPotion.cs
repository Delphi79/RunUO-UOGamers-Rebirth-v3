using System;
using Server;
using Server.Network;

namespace Server.Items
{
	public abstract class BaseHealPotion : BasePotion
	{
		public abstract int MinHeal { get; }
		public abstract int MaxHeal { get; }

		public BaseHealPotion( PotionEffect effect ) : base( 0xF0C, effect )
		{
		}

		public BaseHealPotion( Serial serial ) : base( serial )
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

		public void DoHeal( Mobile from )
		{
			int min = Scale( from, MinHeal );
			int max = Scale( from, MaxHeal );

			from.Heal( Utility.RandomMinMax( min, max ) );
		}

		public override void Drink( Mobile from )
		{
			if ( from.Hits < from.HitsMax )
			{
				if ( from.BeginAction( typeof( BaseHealPotion ) ) )
				{
					DoHeal( from );

					BasePotion.PlayDrinkEffect( from );

					this.Delete();

					Timer.DelayCall( TimeSpan.FromSeconds( 10.0 ), new TimerStateCallback( ReleaseHealLock ), from );
				}
				else
				{
					from.LocalOverheadMessage( MessageType.Regular, 0x22, 500235 ); // You must wait 10 seconds before using another healing potion.
				}
			}
			else
			{
				from.SendLocalizedMessage( 1049547 ); // You decide against drinking this potion, as you are already at full health.
			}
		}

		private static void ReleaseHealLock( object state )
		{
			((Mobile)state).EndAction( typeof( BaseHealPotion ) );
		}
	}
}