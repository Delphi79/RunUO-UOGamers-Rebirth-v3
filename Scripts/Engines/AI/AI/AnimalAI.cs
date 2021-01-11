using System;
using System.Collections; using System.Collections.Generic;
using Server.Targeting;
using Server.Network;

// Ideas
// When you run on animals the panic
// When if ( distance < 8 && Utility.RandomDouble() * Math.Sqrt( (8 - distance) / 6 ) >= incoming.Skills[SkillName.AnimalTaming].Value )
// More your close, the more it can panic
/*
 * AnimalHunterAI, AnimalHidingAI, AnimalDomesticAI...
 * 
 */ 

namespace Server.Mobiles
{
	public class AnimalAI : BaseAI
	{
		public AnimalAI(BaseCreature m) : base (m)
		{
		}

		public override bool DoActionWander()
		{
			if ( m_Mobile.CheckFlee() ) // Less than 10% health
			{
				m_Mobile.DebugSay( "I am low on health!" );
				Action = ActionType.Flee;
			}
			else if ( AquireFocusMob( m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true ) )
			{
				if ( m_Mobile.Debug )
					m_Mobile.DebugSay( "I have detected {0}, attacking", m_Mobile.FocusMob.Name );

				m_Mobile.Combatant = m_Mobile.FocusMob;
				Action = ActionType.Combat;
			}
			else
			{
				base.DoActionWander();
			}

			return true;
		}

		public override bool DoActionCombat()
		{
			Mobile combatant = m_Mobile.Combatant;

			if ( combatant == null || combatant.Deleted || !m_Mobile.InRange( combatant.Location, m_Mobile.RangePerception+1 ) || !combatant.Alive || combatant.Map != m_Mobile.Map )
			{
				m_Mobile.DebugSay( "My combatant is gone.." );

				Action = ActionType.Wander;

				return true;
			}

			if ( WalkMobileRange( combatant, 1, false, m_Mobile.RangeFight, m_Mobile.RangeFight ) )
			{
				m_Mobile.Direction = m_Mobile.GetDirectionTo( combatant );
			}
			else
			{
				if ( m_Mobile.GetDistanceToSqrt( combatant ) > m_Mobile.RangePerception + 1 )
				{
					if ( m_Mobile.Debug )
						m_Mobile.DebugSay( "I cannot find {0}", combatant.Name );

					Action = ActionType.Wander;

					return true;
				}
				else
				{
					if ( m_Mobile.Debug )
						m_Mobile.DebugSay( "I should be closer to {0}", combatant.Name );
				}
			}

			if ( m_Mobile.CheckFlee() )
			{
				m_Mobile.DebugSay( "I am low on health!" );
				Action = ActionType.Flee;
			}

			return true;
		}

		public override bool DoActionBackoff()
		{
			double hitPercent = (double)m_Mobile.Hits / m_Mobile.HitsMax;

			if ( !m_Mobile.Summoned && !m_Mobile.Controled && hitPercent < 0.1 ) // Less than 10% health
			{
				Action = ActionType.Flee;
			}
			else
			{
				if (AquireFocusMob(m_Mobile.RangePerception * 2, FightMode.Closest, true, false , true))
				{
					if ( WalkMobileRange(m_Mobile.FocusMob, 1, false, m_Mobile.RangePerception, m_Mobile.RangePerception * 2) )
					{
						m_Mobile.DebugSay( "Well, here I am safe" );
						Action = ActionType.Wander;
					}					
				}
				else
				{
					m_Mobile.DebugSay( "I have lost my focus, lets relax" );
					Action = ActionType.Wander;
				}
			}

			return true;
		}

		public override bool DoActionFlee()
		{
			AquireFocusMob(m_Mobile.RangePerception * 2, m_Mobile.FightMode, true, false, true);

			if ( m_Mobile.FocusMob == null )
				m_Mobile.FocusMob = m_Mobile.Combatant;

			return base.DoActionFlee();
		}
	}
}
