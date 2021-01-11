using System;
using Server.Targeting;
using Server.Network;

namespace Server.Spells.Sixth
{
	public class ExplosionSpell : Spell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Explosion", "Vas Ort Flam",
				SpellCircle.Sixth,
				230,
				9041,
				Reagent.Bloodmoss,
				Reagent.MandrakeRoot
			);

		public ExplosionSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public override bool DelayedDamage{ get{ return false; } }

		public void Target( Mobile m )
		{
			if ( !Caster.CanSee( m ) )
			{
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.
			}
			else if ( Caster.CanBeHarmful( m ) && CheckSequence() )
			{
				Mobile attacker = Caster, defender = m;
				SpellHelper.Turn( Caster, m );
				SpellHelper.CheckReflect( (int)this.Circle, Caster, ref m );

				if ( attacker.HarmfulCheck( defender ) )
				{
					attacker.DoHarmful( defender, false );
					InternalTimer t = new InternalTimer( this, attacker, defender, m );
					t.Start();
				}
			}

			FinishSequence();
		}

		private class InternalTimer : Timer
		{
			private Spell m_Spell;
			private Mobile m_Target;
			private Mobile m_Attacker, m_Defender;

			public InternalTimer( Spell spell, Mobile attacker, Mobile defender, Mobile target ) : base( TimeSpan.FromSeconds( 3.2 ) )
			{
				m_Spell = spell;
				m_Attacker = attacker;
				m_Defender = defender;
				m_Target = target;

				Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{
				double damage = m_Spell.GetDamage( m_Target );
				
				m_Target.FixedParticles( 0x36BD, 20, 10, 5044, EffectLayer.Head );
				m_Target.PlaySound( 0x307 );

				SpellHelper.Damage( m_Spell, m_Target, damage, 0, 100, 0, 0, 0 );
			}
		}

		private class InternalTarget : Target
		{
			private ExplosionSpell m_Owner;

			public InternalTarget( ExplosionSpell owner ) : base( 12, false, TargetFlags.Harmful )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
					m_Owner.Target( (Mobile)o );
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}