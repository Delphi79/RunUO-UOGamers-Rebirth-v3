using System;
using Server.Items;

namespace Server.SkillHandlers
{
	class Meditation
	{
		public static void Initialize()
		{
			SkillInfo.Table[46].Callback = new SkillUseCallback( OnUse );
		}

		public static bool CheckOkayHolding( Item item )
		{
			if ( item == null )
				return true;

			if ( item is Spellbook )
				return true;

			if ( Core.AOS && item is BaseWeapon && ((BaseWeapon)item).Attributes.SpellChanneling != 0 )
				return true;

			if ( Core.AOS && item is BaseArmor && ((BaseArmor)item).Attributes.SpellChanneling != 0 )
				return true;

			return false;
		}

		public static TimeSpan OnUse( Mobile m )
		{
			m.SendAsciiMessage( "That skill has been disabled for historical accuracy." );
			return TimeSpan.Zero;

			/*m.RevealingAction();

			if ( m.Target != null )
			{
				m.SendLocalizedMessage( 501845 ); // You are busy doing something else and cannot focus.

				return TimeSpan.FromSeconds( 5.0 );
			} 
			else if ( m.Hits < (m.HitsMax / 10) ) // Less than 10% health
			{
				m.SendLocalizedMessage( 501849 ); // The mind is strong but the body is weak.

				return TimeSpan.FromSeconds( 5.0 );
			}
			else if ( m.Mana >= m.ManaMax )
			{
				m.SendLocalizedMessage( 501846 ); // You are at peace.

				return TimeSpan.FromSeconds( 5.0 );
			}
			else 
			{
				Item oneHanded = m.FindItemOnLayer( Layer.OneHanded );
				Item twoHanded = m.FindItemOnLayer( Layer.TwoHanded );

				if ( Core.AOS )
				{
					if ( !CheckOkayHolding( oneHanded ) )
						m.AddToBackpack( oneHanded );

					if ( !CheckOkayHolding( twoHanded ) )
						m.AddToBackpack( twoHanded );
				}
				else if ( !CheckOkayHolding( oneHanded ) || !CheckOkayHolding( twoHanded ) )
				{
					m.SendLocalizedMessage( 502626 ); // Your hands must be free to cast spells or meditate.

					return TimeSpan.FromSeconds( 2.5 );
				}

				if ( m.CheckSkill( SkillName.Meditation, 0, 100 ) )
				{
					m.SendLocalizedMessage( 501851 ); // You enter a meditative trance.
					m.Meditating = true;

					if ( m.Player || m.Body.IsHuman )
						m.PlaySound( 0xF9 );
				} 
				else 
				{
					m.SendLocalizedMessage( 501850 ); // You cannot focus your concentration.
				}

				return TimeSpan.FromSeconds( 10.0 );
			}*/
		}
	}
}