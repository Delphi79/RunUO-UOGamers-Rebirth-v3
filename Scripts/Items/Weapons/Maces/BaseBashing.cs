using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public abstract class BaseBashing : BaseMeleeWeapon
	{
		public override int DefHitSound{ get{ return 0x233; } }
		public override int DefMissSound{ get{ return 0x239; } }

		public override SkillName DefSkill{ get{ return SkillName.Macing; } }
		public override WeaponType DefType{ get{ return WeaponType.Bashing; } }
		public override WeaponAnimation DefAnimation{ get{ return WeaponAnimation.Bash1H; } }

		public BaseBashing( int itemID ) : base( itemID )
		{
		}

		public BaseBashing( Serial serial ) : base( serial )
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

		public override void OnHit( Mobile attacker, Mobile defender )
		{
			int hp = defender.Hits;
			base.OnHit( attacker, defender );
			hp -= defender.Hits;

			if ( hp > 0 )
			{
				int loss = ( hp / 3 ) + Utility.RandomMinMax( -1, 1 );
				if ( loss <= 1 )
					loss = 1;
				else if ( loss > 10 )
					loss = 10;

				defender.Stam -= loss;
			}
		}
	}
}
