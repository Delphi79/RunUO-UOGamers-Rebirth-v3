using System;
using System.Collections; using System.Collections.Generic;
using Server;
using Server.Network;

namespace Server.Items
{
	public abstract class WeaponAbility
	{
		public virtual int BaseMana{ get{ return 0; } }

		public virtual double AccuracyScalar{ get{ return 1.0; } }
		public virtual double DamageScalar{ get{ return 1.0; } }
		public virtual bool NoDamageAbsorb{ get{ return false; } }

		public virtual void OnHit( Mobile attacker, Mobile defender, int damage )
		{
		}

		public virtual void OnMiss( Mobile attacker, Mobile defender )
		{
		}

		public virtual double GetRequiredSkill( Mobile from )
		{
			BaseWeapon weapon = from.Weapon as BaseWeapon;

			if ( weapon != null && weapon.PrimaryAbility == this )
				return 70.0;
			else if ( weapon != null && weapon.SecondaryAbility == this )
				return 90.0;

			return 200.0;
		}

		public virtual int CalculateMana( Mobile from )
		{
			int mana = BaseMana;

			double skillTotal = GetSkill( from, SkillName.Swords ) + GetSkill( from, SkillName.Macing )
				+ GetSkill( from, SkillName.Fencing ) + GetSkill( from, SkillName.Archery ) + GetSkill( from, SkillName.Parry )
				+ GetSkill( from, SkillName.Lumberjacking ) + GetSkill( from, SkillName.Stealth )
				+ GetSkill( from, SkillName.Poisoning );

			if ( skillTotal >= 300.0 )
				mana -= 10;
			else if ( skillTotal >= 200.0 )
				mana -= 5;

			return mana;
		}

		public virtual bool CheckWeaponSkill( Mobile from )
		{
			BaseWeapon weapon = from.Weapon as BaseWeapon;

			if ( weapon == null )
				return false;

			Skill skill = from.Skills[weapon.Skill];
			double reqSkill = GetRequiredSkill( from );

			if ( skill != null && skill.Base >= reqSkill )
				return true;

			from.SendLocalizedMessage( 1060182, reqSkill.ToString() ); // You need ~1_SKILL_REQUIREMENT~ weapon skill to perform that attack

			return false;
		}

		public virtual bool CheckSkills( Mobile from )
		{
			return CheckWeaponSkill( from );
		}

		public virtual double GetSkill( Mobile from, SkillName skillName )
		{
			Skill skill = from.Skills[skillName];

			if ( skill == null )
				return 0.0;

			return skill.Value;
		}

		public virtual bool CheckMana( Mobile from, bool consume )
		{
			int mana = CalculateMana( from );

			if ( from.Mana < mana )
			{
				from.SendLocalizedMessage( 1060181, mana.ToString() ); // You need ~1_MANA_REQUIREMENT~ mana to perform that attack
				return false;
			}

			if ( consume )
				from.Mana -= mana;

			return true;
		}

		public virtual bool Validate( Mobile from )
		{
			return CheckSkills( from ) && CheckMana( from, false );
		}

		private static WeaponAbility[] m_Abilities = new WeaponAbility[14]
			{
				null,
				new ArmorIgnore(),
				new BleedAttack(),
				new ConcussionBlow(),
				new CrushingBlow(),
				new Disarm(),
				new Dismount(),
				new DoubleStrike(),
				new InfectiousStrike(),
				new MortalStrike(),
				new MovingShot(),
				new ParalyzingBlow(),
				new ShadowStrike(),
				new WhirlwindAttack()
			};

		public static WeaponAbility[] Abilities{ get{ return m_Abilities; } }

		private static Hashtable m_Table = new Hashtable();

		public static Hashtable Table{ get{ return m_Table; } }

		public static readonly WeaponAbility ArmorIgnore		= m_Abilities[ 1];
		public static readonly WeaponAbility BleedAttack		= m_Abilities[ 2];
		public static readonly WeaponAbility ConcussionBlow		= m_Abilities[ 3];
		public static readonly WeaponAbility CrushingBlow		= m_Abilities[ 4];
		public static readonly WeaponAbility Disarm				= m_Abilities[ 5];
		public static readonly WeaponAbility Dismount			= m_Abilities[ 6];
		public static readonly WeaponAbility DoubleStrike		= m_Abilities[ 7];
		public static readonly WeaponAbility InfectiousStrike	= m_Abilities[ 8];
		public static readonly WeaponAbility MortalStrike		= m_Abilities[ 9];
		public static readonly WeaponAbility MovingShot			= m_Abilities[10];
		public static readonly WeaponAbility ParalyzingBlow		= m_Abilities[11];
		public static readonly WeaponAbility ShadowStrike		= m_Abilities[12];
		public static readonly WeaponAbility WhirlwindAttack	= m_Abilities[13];

		public static bool IsWeaponAbility( Mobile m, WeaponAbility a )
		{
			if ( a == null )
				return true;

			BaseWeapon weapon = m.Weapon as BaseWeapon;

			return ( weapon != null && (weapon.PrimaryAbility == a || weapon.SecondaryAbility == a) );
		}

		public static WeaponAbility GetCurrentAbility( Mobile m )
		{
			if ( !Core.AOS )
			{
				ClearCurrentAbility( m );
				return null;
			}

			WeaponAbility a = (WeaponAbility)m_Table[m];

			if ( !IsWeaponAbility( m, a ) )
			{
				ClearCurrentAbility( m );
				return null;
			}

			if ( a != null && !a.Validate( m ) )
			{
				ClearCurrentAbility( m );
				return null;
			}

			return a;
		}

		public static bool SetCurrentAbility( Mobile m, WeaponAbility a )
		{
			if ( !Core.AOS )
			{
				ClearCurrentAbility( m );
				return false;
			}

			if ( !IsWeaponAbility( m, a ) )
			{
				ClearCurrentAbility( m );
				return false;
			}

			if ( a != null && !a.Validate( m ) )
			{
				ClearCurrentAbility( m );
				return false;
			}

			if ( a == null )
				m_Table.Remove( m );
			else
				m_Table[m] = a;

			return true;
		}

		public static void ClearCurrentAbility( Mobile m )
		{
			m_Table[m] = null;

			if ( m.NetState != null )
				m.Send( ClearWeaponAbility.Instance );
		}

		public static void Initialize()
		{
			EventSink.SetAbility += new SetAbilityEventHandler( EventSink_SetAbility );
		}

		public WeaponAbility()
		{
		}

		private static void EventSink_SetAbility( SetAbilityEventArgs e )
		{
			int index = e.Index;

			if ( index == 0 )
				ClearCurrentAbility( e.Mobile );
			else if ( index >= 1 && index < m_Abilities.Length )
				SetCurrentAbility( e.Mobile, m_Abilities[index] );
		}
	}
}