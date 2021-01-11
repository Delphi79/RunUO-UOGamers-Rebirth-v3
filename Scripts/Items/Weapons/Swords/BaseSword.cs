using System;
using Server;
using Server.Items;
using Server.Targets;

namespace Server.Items
{
	public abstract class BaseSword : BaseMeleeWeapon
	{
		public override SkillName DefSkill{ get{ return SkillName.Swords; } }
		public override WeaponType DefType{ get{ return WeaponType.Slashing; } }
		public override WeaponAnimation DefAnimation{ get{ return WeaponAnimation.Slash1H; } }

		public BaseSword( int itemID ) : base( itemID )
		{
		}

		public BaseSword( Serial serial ) : base( serial )
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

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.InRange( this.GetWorldLocation(), 3 )	)
			{
				from.SendLocalizedMessage( 1010018 ); // What do you want to use this item on?
				from.Target = new BladedItemTarget( this );
			}
		}

		public override void OnHit( Mobile attacker, Mobile defender )
		{
			base.OnHit( attacker, defender );

			if ( !Core.AOS && Poison != null && PoisonCharges > 0 && !defender.Poisoned )
			{
				--PoisonCharges;

				if ( Utility.RandomDouble() < PoisonChance ) 
				{
					defender.SayTo( defender,  true, "{0} has just poisoned you!", attacker.Name );
					defender.ApplyPoison( attacker, Poison );
				}
			}
		}
	}
}
