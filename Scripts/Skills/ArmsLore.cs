using System;
using Server.Targeting;
using Server.Items;

namespace Server.SkillHandlers
{
	public class ArmsLore
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.ArmsLore].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse(Mobile m)
		{
			m.Target = new InternalTarget();

			m.SendLocalizedMessage( 500349 ); // What item do you wish to get information about?

			return TimeSpan.FromSeconds( 10.0 );
		}

		private class InternalTarget : Target
		{
			public InternalTarget() : base( 2, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is BaseWeapon )
				{
					if ( from.CheckTargetSkill( SkillName.ArmsLore, targeted, 0, 100 ) )
					{
						BaseWeapon weap = (BaseWeapon)targeted;

						if ( weap.MaxHits != 0 )
						{
							int hp = (int)((weap.Hits / (double)weap.MaxHits) * 10);

							if ( hp < 0 )
								hp = 0;
							else if ( hp > 9 )
								hp = 9;

							from.SendLocalizedMessage( 1038285 + hp );
						}

						int damage = (weap.MaxDamage + weap.MinDamage) / 2;
						int hand = (weap.Layer == Layer.OneHanded ? 0 : 1);

						if ( damage < 3 )
							damage = 0;
						else if ( damage < 6 )
							damage = 1;
						else if ( damage < 11 )
							damage = 2;
						else if ( damage < 16 )
							damage = 3;
						else if ( damage < 21 )
							damage = 4;
						else if ( damage < 26 )
							damage = 5;
						else
							damage = 6;

						WeaponType type = weap.Type;

						if ( type == WeaponType.Ranged )
							from.SendLocalizedMessage( 1038224 + (damage * 9) );
						else if ( type == WeaponType.Piercing )
							from.SendLocalizedMessage( 1038218 + hand + (damage * 9) );
						else if ( type == WeaponType.Slashing )
							from.SendLocalizedMessage( 1038220 + hand + (damage * 9) );
						else if ( type == WeaponType.Bashing )
							from.SendLocalizedMessage( 1038222 + hand + (damage * 9) );
						else
							from.SendLocalizedMessage( 1038216 + hand + (damage * 9) );

						if ( weap.Poison != null && weap.PoisonCharges > 0 )
							from.SendLocalizedMessage( 1038284 ); // It appears to have poison smeared on it.
					}
					else
					{
						from.SendLocalizedMessage( 500353 ); // You are not certain...
					}
				}
				else if(targeted is BaseArmor)
				{
					if( from.CheckTargetSkill(SkillName.ArmsLore, targeted, 0, 100) )
					{
						BaseArmor arm = (BaseArmor)targeted;

						if ( arm.MaxHitPoints != 0 )
						{
							int hp = (int)((arm.HitPoints / (double)arm.MaxHitPoints) * 10);

							if ( hp < 0 )
								hp = 0;
							else if ( hp > 9 )
								hp = 9;

							from.SendLocalizedMessage( 1038285 + hp );
						}

						if ( arm.UnscaledArmorRating < 1 )
							from.SendLocalizedMessage( 1038295 ); // This armor offers no defense against attackers.
						else if ( arm.UnscaledArmorRating < 6 )
							from.SendLocalizedMessage( 1038296 ); // This armor provides almost no protection.
						else if ( arm.UnscaledArmorRating < 11 )
							from.SendLocalizedMessage( 1038297 ); // This armor provides very little protection.
						else if ( arm.UnscaledArmorRating < 16 )
							from.SendLocalizedMessage( 1038298 ); // This armor offers some protection against blows.
						else if ( arm.UnscaledArmorRating < 21 )
							from.SendLocalizedMessage( 1038299 ); // This armor serves as sturdy protection.
						else if ( arm.UnscaledArmorRating < 26 )
							from.SendLocalizedMessage( 1038300 ); // This armor is a superior defense against attack.
						else if ( arm.UnscaledArmorRating < 35 )
							from.SendLocalizedMessage( 1038301 ); // This armor offers excellent protection.
						else
							from.SendLocalizedMessage( 1038302 ); // This armor is superbly crafted to provide maximum protection.
					}
					else
					{
						from.SendLocalizedMessage( 500353 ); // You are not certain...
					}
				}
				else
				{
					from.SendLocalizedMessage( 500352 ); // This is neither weapon nor armor.
				}
			}
		}
	}
}