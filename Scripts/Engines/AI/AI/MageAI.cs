using System;
using System.Collections; using System.Collections.Generic;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;
using Server.Spells;
using Server.Spells.First;
using Server.Spells.Second;
using Server.Spells.Third;
using Server.Spells.Fourth;
using Server.Spells.Fifth;
using Server.Spells.Sixth;
using Server.Spells.Seventh;
using Server.Misc;
using Server.Regions;
using Server.SkillHandlers;

namespace Server.Mobiles
{
	public class MageAI : BaseAI
	{
		private DateTime m_NextCastTime;

		public MageAI( BaseCreature m ) : base( m )
		{
		}

		public override bool Think()
		{
			if ( m_Mobile.Deleted )
				return false;

			Target targ = m_Mobile.Target;

			if ( targ != null )
			{
				ProcessTarget( targ );

				return true;
			}
			else
			{
				return base.Think();
			}
		}

		public virtual bool SmartAI
		{
			get{ return ( m_Mobile.Body.IsHuman || m_Mobile is BaseVendor || m_Mobile is BaseEscortable ); }
		}

		public override bool DoActionWander()
		{
			if ( AquireFocusMob( m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true ) )
			{
				m_Mobile.DebugSay( "I am going to attack {0}", m_Mobile.FocusMob.Name );

				m_Mobile.Combatant = m_Mobile.FocusMob;
				Action = ActionType.Combat;
				m_NextCastTime = DateTime.Now;
			}
			else
			{
				if ( m_Mobile.Target != null )
					m_Mobile.Target.Cancel( m_Mobile, TargetCancelType.Canceled );
				m_Mobile.DebugSay( "I am wandering" );

				m_Mobile.Warmode = false;

				base.DoActionWander();
			}

			return true;
		}

		public void RunTo( Mobile m )
		{
			if ( !SmartAI )
			{
				if ( !MoveTo( m, true, m_Mobile.RangeFight ) )
					OnFailedMove();

				return;
			}

			if ( m.Paralyzed || m.Frozen )
			{
				if ( m_Mobile.InRange( m, 1 ) )
					RunFrom( m );
				else if ( !m_Mobile.InRange( m, m_Mobile.RangeFight > 2 ? m_Mobile.RangeFight : 2 ) && !MoveTo( m, true, 1 ) )
					OnFailedMove();
			}
			else
			{
				if ( !m_Mobile.InRange( m, m_Mobile.RangeFight ) )
				{
					if ( !MoveTo( m, true, 1 ) )
						OnFailedMove();
				}
				else if ( m_Mobile.InRange( m, m_Mobile.RangeFight - 1 ) )
				{
					RunFrom( m );
				}
			}
		}

		public void RunFrom( Mobile m )
		{
			Run( (m_Mobile.GetDirectionTo( m ) - 4) & Direction.Mask );
		}

		public void OnFailedMove()
		{
			if ( AquireFocusMob( m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true ) )
			{
				m_Mobile.DebugSay( "My move is blocked, so I am going to attack {0}", m_Mobile.FocusMob.Name );

				m_Mobile.Combatant = m_Mobile.FocusMob;
				Action = ActionType.Combat;
			}
			else
			{
				m_Mobile.DebugSay( "I am stuck" );
			}
		}

		public void Run( Direction d )
		{
			if ( (m_Mobile.Spell != null && m_Mobile.Spell.IsCasting) || m_Mobile.Paralyzed || m_Mobile.Frozen || m_Mobile.DisallowAllMoves )
				return;

			m_Mobile.Direction = d | Direction.Running;

			if ( !DoMove( m_Mobile.Direction ) )
				OnFailedMove();
		}

		public virtual Spell GetRandomDamageSpell( bool canDispel )
		{
			if ( m_Mobile.Mana < 8 )
				return null;

			int maxCircle = (int)( m_Mobile.Skills[SkillName.Magery].Value / 87.5 * 8.0 );
			if ( maxCircle > 8 )
				maxCircle = 8;

			while ( maxCircle > 1 && Spell.m_ManaTable[maxCircle-1] > (m_Mobile.Mana/2) )
				maxCircle--;

			if ( maxCircle < 1 )
				maxCircle = 1;

			int spell;
			if ( maxCircle < 3 )
			{
				spell = maxCircle;
			}
			else
			{
				spell = Utility.Random( maxCircle ) + 1;
			}
			
			switch ( spell )
			{
				default:
				case 1: 
				{
					switch ( Utility.Random( 9 ) )
					{
						case 0: return new ClumsySpell( m_Mobile, null );
						case 1: return new FeeblemindSpell( m_Mobile, null );
						case 2: return new WeakenSpell( m_Mobile, null );
						default: return new MagicArrowSpell( m_Mobile, null );
					}
				}

				case 2: 
					return new HarmSpell( m_Mobile, null );

				case 3: 
				{
					switch ( Utility.Random( 4 ) )
					{
						case 0: return new PoisonSpell( m_Mobile, null );
						default: return new FireballSpell( m_Mobile, null );
					}
				}

				case 4: 
					return new LightningSpell( m_Mobile, null );

				case 5: 
				{
					if ( m_Mobile.Body.IsHuman )
					{
						switch ( Utility.Random( 4 ) )
						{
							case 0: return new ParalyzeSpell( m_Mobile, null );
							default: return new MindBlastSpell( m_Mobile, null );
						}
					}
					else 
					{
						goto case 4;
					}
				}

				case 6: 
				{
					if ( canDispel && Utility.RandomBool() )
						return new DispelSpell( m_Mobile, null );

					switch ( Utility.Random( 2 ) )
					{
						case 0: return new EnergyBoltSpell( m_Mobile, null );
						default: return new ExplosionSpell( m_Mobile, null );
					}
				}

				case 7: 
				case 8:
					return new FlameStrikeSpell( m_Mobile, null );
			}
		}

		protected int m_Combo = -1;

		public virtual Spell DoCombo( Mobile c )
		{
			Spell spell = null;

			if ( m_Combo == 0 )
			{
				spell = new ExplosionSpell( m_Mobile, null );
				++m_Combo; // Move to next spell
			}
			else if ( m_Combo == 1 )
			{
				spell = new WeakenSpell( m_Mobile, null );
				++m_Combo; // Move to next spell
			}
			else if ( m_Combo == 2 )
			{
				if ( !c.Poisoned )
					spell = new PoisonSpell( m_Mobile, null );

				++m_Combo; // Move to next spell
			}

			if ( m_Combo == 3 && spell == null )
			{
				switch ( Utility.Random( 3 ) )
				{
					default:
					case 0:
					{
						if ( c.Int < c.Dex )
							spell = new FeeblemindSpell( m_Mobile, null );
						else
							spell = new ClumsySpell( m_Mobile, null );

						++m_Combo; // Move to next spell

						break;
					}
					case 1:
					{
						spell = new EnergyBoltSpell( m_Mobile, null );
						m_Combo = -1; // Reset combo state
						break;
					}
					case 2:
					{
						spell = new FlameStrikeSpell( m_Mobile, null );
						m_Combo = -1; // Reset combo state
						break;
					}
				}
			}
			else if ( m_Combo == 4 && spell == null )
			{
				spell = new LightningSpell( m_Mobile, null );//MindBlastSpell( m_Mobile, null );
				m_Combo = -1;
			}

			return spell;
		}

		public override bool DoActionCombat()
		{
			Mobile c = m_Mobile.Combatant;
			m_Mobile.Warmode = true;

			if ( c == null || c.Deleted || !c.Alive || !m_Mobile.CanSee( c ) || !m_Mobile.CanBeHarmful( c, false ) || c.Map != m_Mobile.Map )
			{
				// Our combatant is deleted, dead, hidden, or we cannot hurt them
				// Try to find another combatant

				if ( AquireFocusMob( m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true ) )
				{
					m_Mobile.DebugSay( "Something happened to my combatant, so I am going to fight {0}", m_Mobile.FocusMob.Name );

					m_Mobile.Combatant = c = m_Mobile.FocusMob;
					m_Mobile.FocusMob = null;
				}
				else
				{
					m_Mobile.DebugSay( "Something happened to my combatant, and nothing is around. I am on guard." );
					Action = ActionType.Wander;
					return true;
				}
			}
			
			if ( !m_Mobile.InLOS( c ) )
			{
				if ( AquireFocusMob( m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true ) )
				{
					m_Mobile.Combatant = c = m_Mobile.FocusMob;
					m_Mobile.FocusMob = null;
				}
			}

			//if ( SmartAI && !m_Mobile.StunReady && m_Mobile.Skills[SkillName.Wrestling].Value >= 80.0 && m_Mobile.Skills[SkillName.Anatomy].Value >= 80.0 )
			//	EventSink.InvokeStunRequest( new StunRequestEventArgs( m_Mobile ) );

			if ( !m_Mobile.InRange( c, m_Mobile.RangePerception ) )
			{
				// They are somewhat far away, can we find something else?

				if ( AquireFocusMob( m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true ) )
				{
					m_Mobile.Combatant = m_Mobile.FocusMob;
					m_Mobile.FocusMob = null;
				}
				else //if ( !m_Mobile.InRange( c, m_Mobile.RangePerception * 3 ) )
				{
					m_Mobile.Combatant = null;
				}

				c = m_Mobile.Combatant;

				if ( c == null )
				{
					m_Mobile.DebugSay( "My combatant has fled, so I am on guard" );
					Action = ActionType.Wander;

					return true;
				}
			}

			if ( m_Mobile.CheckFlee() )
			{
				// We are low on health, should we flee?

				bool flee = false;

				if ( m_Mobile.Hits < c.Hits )
				{
					// We are more hurt than them

					int diff = c.Hits - m_Mobile.Hits;

					flee = ( Utility.Random( 0, 100 ) > (10 + diff) ); // (10 + diff)% chance to flee
				}
				else
				{
					flee = Utility.Random( 0, 100 ) > 10; // 10% chance to flee
				}

				if ( flee )
				{
					m_Mobile.DebugSay( "I am going to flee from {0}", c.Name );
					Action = ActionType.Flee;
					return true;
				}
			}

			if ( m_Mobile.Spell == null && DateTime.Now > m_NextCastTime && m_Mobile.InRange( c, 12 ) && m_Mobile.InLOS( c ) )
			{
				// We are ready to cast a spell

				DamageEntry de = m_Mobile.FindDamageEntryFor( c );
				if ( de == null || de.HasExpired )
				{
					de = c.FindDamageEntryFor( m_Mobile );
					if ( ( de == null || de.HasExpired ) )
					{
						if ( !NotorietyHandlers.CheckAggressor( m_Mobile.Aggressors, c ) && !NotorietyHandlers.CheckAggressed( c.Aggressed, m_Mobile ) )
						{
							// we cant cast because we didnt give or take damage yet
							RunTo( c );
							return true;
						}
					}
				}

				Spell spell = null;
				Mobile toDispel = FindDispelTarget( true );

				if ( SmartAI && m_Combo != -1 ) // We are doing a spell combo
					spell = DoCombo( c );
				else
					spell = GetRandomDamageSpell( toDispel != null );

				// Now we have a spell picked
				// Move first before casting

				if ( SmartAI && toDispel != null )
				{
					if ( m_Mobile.InRange( toDispel, 5 ) )
						RunFrom( toDispel );
					else if ( !m_Mobile.InRange( toDispel, 11 ) )
						RunTo( toDispel );
				}
				else
				{
					RunTo( c );
				}

				if ( spell != null && spell.Cast() )
				{
					TimeSpan delay;

					if ( spell is DispelSpell )
						delay = TimeSpan.FromSeconds( m_Mobile.ActiveSpeed*2.0 );
					else
						delay = spell.GetCastDelay() + TimeSpan.FromSeconds( Utility.Random( 3 ) );
					m_NextCastTime = DateTime.Now + delay;
				}
			}
			else if ( m_Mobile.Spell == null || !m_Mobile.Spell.IsCasting )
			{
				RunTo( c );
			}

			return true;
		}

		public override bool DoActionFlee()
		{
			if ( m_Mobile.Hits > m_Mobile.HitsMax/2 )
			{
				m_Mobile.DebugSay( "I am stronger now, so I will wander" );
				Action = ActionType.Wander;
			}
			else
			{
				m_Mobile.FocusMob = m_Mobile.Combatant;
				base.DoActionFlee();
			}

			return true;
		}

		public override bool DoActionGuard()
		{
			if ( AquireFocusMob( m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true ) )
			{
				m_Mobile.DebugSay( "I am going to attack {0}", m_Mobile.FocusMob.Name );
				m_Mobile.Combatant = m_Mobile.FocusMob;
				Action = ActionType.Combat;
			}
			else
			{
				base.DoActionGuard();
			}

			return true;
		}

		public Mobile FindDispelTarget( bool activeOnly )
		{
			if ( m_Mobile.Deleted || !SmartAI || m_Mobile.Int < 95 || CanDispel( m_Mobile ) || m_Mobile.AutoDispel )
				return null;

			if ( activeOnly )
			{
				List<AggressorInfo> aggressed = m_Mobile.Aggressed;
                List<AggressorInfo> aggressors = m_Mobile.Aggressors;

				Mobile active = null;
				double activePrio = 0.0;

				Mobile comb = m_Mobile.Combatant;

				if ( comb != null && !comb.Deleted && m_Mobile.InRange( comb, 12 ) && CanDispel( comb ) )
				{
					active = comb;
					activePrio = m_Mobile.GetDistanceToSqrt( comb );

					if ( activePrio <= 2 )
						return active;
				}

				for ( int i = 0; i < aggressed.Count; ++i )
				{
					AggressorInfo info = (AggressorInfo)aggressed[i];
					Mobile m = (Mobile)info.Defender;

					if ( m != comb && m.Combatant == m_Mobile && m_Mobile.InRange( m, 12 ) && CanDispel( m ) )
					{
						double prio = m_Mobile.GetDistanceToSqrt( m );

						if ( active == null || prio < activePrio )
						{
							active = m;
							activePrio = prio;

							if ( activePrio <= 2 )
								return active;
						}
					}
				}

				for ( int i = 0; i < aggressors.Count; ++i )
				{
					AggressorInfo info = (AggressorInfo)aggressors[i];
					Mobile m = (Mobile)info.Attacker;

					if ( m != comb && m.Combatant == m_Mobile && m_Mobile.InRange( m, 12 ) && CanDispel( m ) )
					{
						double prio = m_Mobile.GetDistanceToSqrt( m );

						if ( active == null || prio < activePrio )
						{
							active = m;
							activePrio = prio;

							if ( activePrio <= 2 )
								return active;
						}
					}
				}

				return active;
			}
			else
			{
				Map map = m_Mobile.Map;

				if ( map != null )
				{
					Mobile active = null, inactive = null;
					double actPrio = 0.0, inactPrio = 0.0;

					Mobile comb = m_Mobile.Combatant;

					if ( comb != null && !comb.Deleted && CanDispel( comb ) )
					{
						active = inactive = comb;
						actPrio = inactPrio = m_Mobile.GetDistanceToSqrt( comb );
					}

					foreach ( Mobile m in m_Mobile.GetMobilesInRange( 12 ) )
					{
						if ( m != m_Mobile && CanDispel( m ) )
						{
							double prio = m_Mobile.GetDistanceToSqrt( m );

							if ( !activeOnly && (inactive == null || prio < inactPrio) )
							{
								inactive = m;
								inactPrio = prio;
							}

							if ( (m_Mobile.Combatant == m || m.Combatant == m_Mobile) && (active == null || prio < actPrio) )
							{
								active = m;
								actPrio = prio;
							}
						}
					}

					return active != null ? active : inactive;
				}
			}

			return null;
		}

		public bool CanDispel( Mobile m )
		{
			return ( m is BaseCreature && ((BaseCreature)m).Summoned && m_Mobile.CanBeHarmful( m, false ) && !((BaseCreature)m).IsAnimatedDead );
		}

		private static int[] m_Offsets = new int[]
			{
				-1, -1,
				-1,  0,
				-1,  1,
				 0, -1,
				 0,  1,
				 1, -1,
				 1,  0,
				 1,  1,

				-2, -2,
				-2, -1,
				-2,  0,
				-2,  1,
				-2,  2,
				-1, -2,
				-1,  2,
				 0, -2,
				 0,  2,
				 1, -2,
				 1,  2,
				 2, -2,
				 2, -1,
				 2,  0,
				 2,  1,
				 2,  2
			};

		private void ProcessTarget( Target targ )
		{
			bool isDispel = ( targ is DispelSpell.InternalTarget );
			bool isParalyze = ( targ is ParalyzeSpell.InternalTarget );

			Mobile toTarget;

			if ( isDispel )
			{
				toTarget = FindDispelTarget( false );

				if ( !SmartAI && toTarget != null )
					RunTo( toTarget );
				else if ( toTarget != null && m_Mobile.InRange( toTarget, 8 ) )
					RunFrom( toTarget );
			}
			else if ( SmartAI && isParalyze )
			{
				toTarget = FindDispelTarget( true );

				if ( toTarget == null )
				{
					toTarget = m_Mobile.Combatant;

					if ( toTarget != null )
						RunTo( toTarget );
				}
			}
			else
			{
				toTarget = m_Mobile.Combatant;

				if ( toTarget != null )
					RunTo( toTarget );
			}

			if ( (targ.Flags & TargetFlags.Harmful) != 0 && toTarget != null )
			{
				if ( (targ.Range == -1 || m_Mobile.InRange( toTarget, targ.Range )) && m_Mobile.CanSee( toTarget ) && m_Mobile.InLOS( toTarget ) )
				{
					targ.Invoke( m_Mobile, toTarget );
				}
				else if ( isDispel )
				{
					targ.Cancel( m_Mobile, TargetCancelType.Canceled );
				}
			}
			else if ( (targ.Flags & TargetFlags.Beneficial) != 0 )
			{
				targ.Invoke( m_Mobile, m_Mobile );
			}
			else
			{
				targ.Cancel( m_Mobile, TargetCancelType.Canceled );
			}
		}
	}
}
