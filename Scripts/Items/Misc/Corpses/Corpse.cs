using System;
using System.Collections; using System.Collections.Generic;
using Server;
using Server.Misc;
using Server.Guilds;
using Server.Mobiles;
using Server.Network;
using Server.ContextMenus;

namespace Server.Items
{
    [Flags]
    public enum CorpseFlag
    {
        None = 0x00000000,

        /// <summary>
        /// Has this corpse been carved?
        /// </summary>
        Carved = 0x00000001,

        /// <summary>
        /// If true, this corpse will not turn into bones
        /// </summary>
        NoBones = 0x00000002,

        /// <summary>
        /// If true, the corpse has turned into bones
        /// </summary>
        IsBones = 0x00000004,

        /// <summary>
        /// Has this corpse yet been visited by a taxidermist?
        /// </summary>
        VisitedByTaxidermist = 0x00000008,

        /// <summary>
        /// Has this corpse yet been used to channel spiritual energy? (AOS Spirit Speak)
        /// </summary>
        Channeled = 0x00000010,

        /// <summary>
        /// Was the owner criminal when he died?
        /// </summary>
        Criminal = 0x00000020,
    }

	public class Corpse : Container, ICarvable
	{
		private Mobile		m_Owner;				// Whos corpse is this?
        private Mobile m_Killer;				// Who killed the owner?
        private CorpseFlag m_Flags;				// @see CorpseFlag

		private List<Mobile>m_Looters;				// Who's looted this corpse?
		private List<Item>	m_EquipItems;			// List of items equiped when the owner died. Ingame, these items display /on/ the corpse, not just inside
		private List<Mobile>m_Aggressors;			// Anyone from this list will be able to loot this corpse; we attacked them, or they attacked us when we were freely attackable

		private string		m_CorpseName;			// Value of the CorpseNameAttribute attached to the owner when he died -or- null if the owner had no CorpseNameAttribute; use "the remains of ~name~"
		private bool		m_NoBones;				// If true, this corpse will not turn into bones

		private bool		m_VisitedByTaxidermist;	// Has this corpse yet been visited by a taxidermist?
		private bool		m_Channeled;			// Has this corpse yet been used to channel spiritual energy? (AOS Spirit Speak)

		// For notoriety:
		private AccessLevel	m_AccessLevel;			// Which AccessLevel the owner had when he died
		private Guild		m_Guild;				// Which Guild the owner was in when he died
		private int			m_Kills;				// How many kills the owner had when he died

		private DateTime	m_TimeOfDeath;			// What time was this corpse created?

        private HairInfo m_Hair;					// This contains the hair of the owner
        private FacialHairInfo m_FacialHair;			// This contains the facial hair of the owner

		public override bool IsDecoContainer
		{
			get{ return false; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime TimeOfDeath
		{
			get{ return m_TimeOfDeath; }
			set{ m_TimeOfDeath = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public AccessLevel AccessLevel
		{
			get{ return m_AccessLevel; }
		}

		public List<Mobile> Aggressors
		{
			get{ return m_Aggressors; }
		}

		public List<Mobile> Looters
		{
			get{ return m_Looters; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Killer
		{
			get{ return m_Killer; }
		}

		public List<Item> EquipItems
		{
			get{ return m_EquipItems; }
		}

		public Guild Guild
		{
			get{ return m_Guild; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int Kills
		{
			get{ return m_Kills; }
			set{ m_Kills = value; }
		}

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Criminal
        {
            get { return GetFlag(CorpseFlag.Criminal); }
            set { SetFlag(CorpseFlag.Criminal, value); }
        }

        public HairInfo Hair { get { return m_Hair; } }
        public FacialHairInfo FacialHair { get { return m_FacialHair; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsBones
        {
            get { return GetFlag(CorpseFlag.IsBones); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Carved
        {
            get { return GetFlag(CorpseFlag.Carved); }
            set { SetFlag(CorpseFlag.Carved, value); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool VisitedByTaxidermist
        {
            get { return GetFlag(CorpseFlag.VisitedByTaxidermist); }
            set { SetFlag(CorpseFlag.VisitedByTaxidermist, value); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Channeled
        {
            get { return GetFlag(CorpseFlag.Channeled); }
            set { SetFlag(CorpseFlag.Channeled, value); }
        }


		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Owner
		{
			get{ return m_Owner; }
		}

		public void TurnToBones()
		{
			if ( Deleted )
				return;

			ProcessDelta();
			SendRemovePacket();
			ItemID = Utility.Random( 0xECA, 9 ); // bone graphic
			Hue = 0;
			ProcessDelta();

			m_NoBones = true;
			BeginDecay( m_BoneDecayTime );

			/*DecayedCorpse c = new DecayedCorpse( Name );

			c.MoveToWorld( Location, Map );

			ArrayList list = Items;

			for ( int i = list.Count - 1; i >= 0; --i )
			{
				if ( i < list.Count )
					c.AddItem( (Item)list[i] );
			}

			Delete();*/
		}

		private static TimeSpan m_DefaultDecayTime = TimeSpan.FromMinutes( 10.0 );
		private static TimeSpan m_BoneDecayTime = TimeSpan.FromMinutes( 5.0 );

		private Timer m_DecayTimer;
		private DateTime m_DecayTime;

		public void BeginDecay( TimeSpan delay )
		{
			if ( m_DecayTimer != null )
				m_DecayTimer.Stop();

			m_DecayTime = DateTime.Now + delay;

			m_DecayTimer = new InternalTimer( this, delay );
			m_DecayTimer.Start();
		}

		public override void OnAfterDelete()
		{
			if ( m_DecayTimer != null )
				m_DecayTimer.Stop();

			m_DecayTimer = null;
		}

		private class InternalTimer : Timer
		{
			private Corpse m_Corpse;

			public InternalTimer( Corpse c, TimeSpan delay ) : base( delay )
			{
				m_Corpse = c;
				Priority = TimerPriority.FiveSeconds;
			}

			protected override void OnTick()
			{
				if ( !m_Corpse.m_NoBones )
					m_Corpse.TurnToBones();
				else
					m_Corpse.Delete();
			}
		}

		public static string GetCorpseName( Mobile m )
		{
			Type t = m.GetType();

			object[] attrs = t.GetCustomAttributes( typeof( CorpseNameAttribute ), true );

			if ( attrs != null && attrs.Length > 0 )
			{
				CorpseNameAttribute attr = attrs[0] as CorpseNameAttribute;

				if ( attr != null )
					return attr.Name;
			}

			return null;
		}

		public static void Initialize()
		{
			Mobile.CreateCorpseHandler += new CreateCorpseHandler( Mobile_CreateCorpseHandler );
		}


        public static Container Mobile_CreateCorpseHandler(Mobile owner, HairInfo hair, FacialHairInfo facialhair, List<Item> initialContent, List<Item> equipedItems)
		{
			bool shouldFillCorpse = true;

			//if ( owner is BaseCreature )
			//	shouldFillCorpse = !((BaseCreature)owner).IsBonded;

            Corpse c = new Corpse(owner, hair, facialhair, shouldFillCorpse ? equipedItems : new List<Item>());

			owner.Corpse = c;

			if ( shouldFillCorpse )
			{
				for ( int i = 0; i < initialContent.Count; ++i )
				{
					Item item = (Item)initialContent[i];

					if ( Core.AOS && owner.Player && item.Parent == owner.Backpack )
						c.AddItem( item );
					else
						c.DropItem( item );

					if ( owner.Player && Core.AOS )
						c.SetRestoreInfo( item, item.Location );
				}
			}
			else
			{
				c.Carved = true; // TODO: Is it needed?
			}

			Point3D loc = owner.Location;
			Map map = owner.Map;

			if ( map == null || map == Map.Internal )
			{
				loc = owner.LogoutLocation;
				map = owner.LogoutMap;
			}

			c.MoveToWorld( loc, map );

			return c;
		}

		public override bool IsPublicContainer{ get{ return true; } }

        public Corpse(Mobile owner, HairInfo hair, FacialHairInfo facialhair, List<Item> equipItems)
           : base( 0x2006 )
		{
			// To supress console warnings, stackable must be true
			Stackable = true;
			Amount = owner.Body; // protocol defines that for itemid 0x2006, amount=body
			Stackable = false;

			Movable = false;
			Hue = owner.Hue;
			Direction = owner.Direction;
			Name = owner.Name;

			m_Owner = owner;

			m_CorpseName = GetCorpseName( owner );

			m_TimeOfDeath = DateTime.Now;

			m_AccessLevel = owner.AccessLevel;
			m_Guild = owner.Guild as Guild;
            m_Kills = owner.Kills;
            SetFlag(CorpseFlag.Criminal, owner.Criminal);

            m_Hair = hair;
            m_FacialHair = facialhair;

#if false
			// This corpse does not turn to bones if:
			//    (the owner is not a player) and (the owner doesn't have a human body)
			m_NoBones = !owner.Player && !owner.Body.IsHuman;
#else
			// This corpse does not turn to bones if:
			//    (the owner is not a player)
			m_NoBones = !owner.Player;
#endif

			m_Looters = new List<Mobile>();
			m_EquipItems = equipItems;

			m_Aggressors = new List<Mobile>( owner.Aggressors.Count + owner.Aggressed.Count );
			bool addToAggressors = !( owner is BaseCreature );

			TimeSpan lastTime = TimeSpan.MaxValue;

			for ( int i = 0; i < owner.Aggressors.Count; ++i )
			{
				AggressorInfo info = (AggressorInfo)owner.Aggressors[i];

				if ( (DateTime.Now - info.LastCombatTime) < lastTime )
				{
					m_Killer = info.Attacker;
					lastTime = (DateTime.Now - info.LastCombatTime);
				}

				if ( addToAggressors && !info.CriminalAggression )
					m_Aggressors.Add( info.Attacker );
			}

			for ( int i = 0; i < owner.Aggressed.Count; ++i )
			{
				AggressorInfo info = (AggressorInfo)owner.Aggressed[i];

				if ( (DateTime.Now - info.LastCombatTime) < lastTime )
				{
					m_Killer = info.Defender;
					lastTime = (DateTime.Now - info.LastCombatTime);
				}

				if ( addToAggressors )
					m_Aggressors.Add( info.Defender );
			}

			if ( !addToAggressors )
			{
				BaseCreature bc = (BaseCreature)owner;

				Mobile master = bc.ControlMaster;
				if( master != null )
					m_Aggressors.Add( master );

				ArrayList rights = BaseCreature.GetLootingRights( bc.DamageEntries );
				for ( int i = 0; i < rights.Count; ++i )
				{
					DamageStore ds = (DamageStore)rights[i];

					if ( ds.m_HasRight )
						m_Aggressors.Add( ds.m_Mobile );
				}
			}

			BeginDecay( m_DefaultDecayTime );
		}

        public Corpse(Serial serial)
            : base(serial)
        {
        }

        protected bool GetFlag(CorpseFlag flag)
        {
            return ((m_Flags & flag) != 0);
        }

        protected void SetFlag(CorpseFlag flag, bool on)
        {
            m_Flags = (on ? m_Flags | flag : m_Flags & ~flag);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)11); // version

            writer.Write((int)m_Flags);

            writer.WriteDeltaTime(m_TimeOfDeath);

            List<KeyValuePair<Item, Point3D>> list = (m_RestoreTable == null ? null : new List<KeyValuePair<Item, Point3D>>(m_RestoreTable));
            int count = (list == null ? 0 : list.Count);

            writer.Write(count);

            for (int i = 0; i < count; ++i)
            {
                KeyValuePair<Item, Point3D> kvp = list[i];
                Item item = kvp.Key;
                Point3D loc = kvp.Value;

                writer.Write(item);

                if (item.Location == loc)
                {
                    writer.Write(false);
                }
                else
                {
                    writer.Write(true);
                    writer.Write(loc);
                }
            }

            writer.Write(m_DecayTimer != null);

            if (m_DecayTimer != null)
                writer.WriteDeltaTime(m_DecayTime);

            writer.Write(m_Looters);
            writer.Write(m_Killer);

            writer.Write(m_Aggressors);

            writer.Write(m_Owner);

            writer.Write((string)m_CorpseName);

            writer.Write((int)m_AccessLevel);
            writer.Write((Guild)m_Guild);
            writer.Write((int)m_Kills);

            writer.Write(m_EquipItems);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch (version)
            {
                case 11:
                    {
                        // Version 11, we move all bools to a CorpseFlag
                        m_Flags = (CorpseFlag)reader.ReadInt();

                        m_TimeOfDeath = reader.ReadDeltaTime();

                        int count = reader.ReadInt();

                        for (int i = 0; i < count; ++i)
                        {
                            Item item = reader.ReadItem();

                            if (reader.ReadBool())
                                SetRestoreInfo(item, reader.ReadPoint3D());
                            else if (item != null)
                                SetRestoreInfo(item, item.Location);
                        }

                        if (reader.ReadBool())
                            BeginDecay(reader.ReadDeltaTime() - DateTime.Now);

                        m_Looters = reader.ReadStrongMobileList();
                        m_Killer = reader.ReadMobile();

                        m_Aggressors = reader.ReadStrongMobileList();
                        m_Owner = reader.ReadMobile();

                        m_CorpseName = reader.ReadString();

                        m_AccessLevel = (AccessLevel)reader.ReadInt();
                        reader.ReadInt(); // guild reserve
                        m_Kills = reader.ReadInt();

                        m_EquipItems = reader.ReadStrongItemList();
                        break;
                    }
                case 10:
                    {
                        m_TimeOfDeath = reader.ReadDeltaTime();

                        goto case 9;
                    }
                case 9:
                    {
                        int count = reader.ReadInt();

                        for (int i = 0; i < count; ++i)
                        {
                            Item item = reader.ReadItem();

                            if (reader.ReadBool())
                                SetRestoreInfo(item, reader.ReadPoint3D());
                            else if (item != null)
                                SetRestoreInfo(item, item.Location);
                        }

                        goto case 8;
                    }
                case 8:
                    {
                        SetFlag(CorpseFlag.VisitedByTaxidermist, reader.ReadBool());

                        goto case 7;
                    }
                case 7:
                    {
                        if (reader.ReadBool())
                            BeginDecay(reader.ReadDeltaTime() - DateTime.Now);

                        goto case 6;
                    }
                case 6:
                    {
                        m_Looters = reader.ReadStrongMobileList();
                        m_Killer = reader.ReadMobile();

                        goto case 5;
                    }
                case 5:
                    {
                        SetFlag(CorpseFlag.Carved, reader.ReadBool());

                        goto case 4;
                    }
                case 4:
                    {
                        m_Aggressors = reader.ReadStrongMobileList();

                        goto case 3;
                    }
                case 3:
                    {
                        m_Owner = reader.ReadMobile();

                        goto case 2;
                    }
                case 2:
                    {
                        SetFlag(CorpseFlag.NoBones, reader.ReadBool());

                        goto case 1;
                    }
                case 1:
                    {
                        m_CorpseName = reader.ReadString();

                        goto case 0;
                    }
                case 0:
                    {
                        if (version < 10)
                            m_TimeOfDeath = DateTime.Now;

                        if (version < 7)
                            BeginDecay(m_DefaultDecayTime);

                        if (version < 6)
                            m_Looters = new List<Mobile>();

                        if (version < 4)
                            m_Aggressors = new List<Mobile>();

                        m_AccessLevel = (AccessLevel)reader.ReadInt();
                        reader.ReadInt(); // guild reserve
                        m_Kills = reader.ReadInt();
                        SetFlag(CorpseFlag.Criminal, reader.ReadBool());

                        m_EquipItems = reader.ReadStrongItemList();

                        break;
                    }
            }
        }

		public override void SendInfoTo( NetState state )
		{
			base.SendInfoTo( state );

			if ( ItemID == 0x2006 )
			{
				state.Send( new CorpseContent( state.Mobile, this ) );
				state.Send( new CorpseEquip( state.Mobile, this ) );
			}
		}

		public bool IsCriminalAction( Mobile from )
		{
			if ( from == m_Owner || from.AccessLevel >= AccessLevel.GameMaster )
				return false;

			return ( NotorietyHandlers.CorpseNotoriety( from, this ) == Notoriety.Innocent );
		}

		public override bool CheckItemUse( Mobile from, Item item )
		{
			if ( !base.CheckItemUse( from, item ) )
				return false;

			if ( item != this )
				return CanLoot( from );

			return true;
		}

		public override bool CheckLift( Mobile from, Item item, ref LRReason reject )
		{
			if ( !base.CheckLift( from, item, ref reject ) )
				return false;

			return CanLoot( from );
		}

		public override void OnItemUsed( Mobile from, Item item )
		{
			base.OnItemUsed( from, item );

			if ( item != this && IsCriminalAction( from ) )
				from.CriminalAction( true );

			if ( !m_Looters.Contains( from ) )
				m_Looters.Add( from );
		}

		public override void OnItemLifted( Mobile from, Item item )
		{
			base.OnItemLifted( from, item );

			if ( item != this && IsCriminalAction( from ) )
				from.CriminalAction( true );

			if ( !m_Looters.Contains( from ) )
				m_Looters.Add( from );
		}

		private class OpenCorpseEntry : ContextMenuEntry
		{
			public OpenCorpseEntry() : base( 6215, 2 )
			{
			}

			public override void OnClick()
			{
				Corpse corpse = Owner.Target as Corpse;

				if ( corpse != null && Owner.From.CheckAlive() )
					corpse.Open( Owner.From, false );
			}
		}

		public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list )
		{
			base.GetContextMenuEntries( from, list );

			if ( Core.AOS && m_Owner == from && from.Alive )
				list.Add( new OpenCorpseEntry() );
		}

        private Dictionary<Item, Point3D> m_RestoreTable;

        public bool GetRestoreInfo(Item item, ref Point3D loc)
        {
            if (m_RestoreTable == null || item == null)
                return false;

            return m_RestoreTable.TryGetValue(item, out loc);
        }

        public void SetRestoreInfo(Item item, Point3D loc)
        {
            if (item == null)
                return;

            if (m_RestoreTable == null)
                m_RestoreTable = new Dictionary<Item, Point3D>();

            m_RestoreTable[item] = loc;
        }

        public void ClearRestoreInfo(Item item)
        {
            if (m_RestoreTable == null || item == null)
                return;

            m_RestoreTable.Remove(item);

            if (m_RestoreTable.Count == 0)
                m_RestoreTable = null;
        }

		public bool CanLoot( Mobile from )
		{
			if ( !IsCriminalAction( from ) )
				return true;

			Map map = this.Map;

			if ( map == null || (map.Rules & MapRules.HarmfulRestrictions) != 0 )
				return false;

			return true;
		}

		public bool CheckLoot( Mobile from )
		{
			if ( !CanLoot( from ) )
			{
				if ( m_Owner == null || !m_Owner.Player )
					from.SendLocalizedMessage( 1005035 ); // You did not earn the right to loot this creature!
				else
					from.SendLocalizedMessage( 1010049 ); // You may not loot this corpse.

				return false;
			}
			else if ( IsCriminalAction( from ) )
			{
				if ( m_Owner == null || !m_Owner.Player )
					from.SendLocalizedMessage( 1005036 ); // Looting this monster corpse will be a criminal act!
				else
					from.SendLocalizedMessage( 1005038 ); // Looting this corpse will be a criminal act!
			}

			return true;
		}

		public virtual void Open( Mobile from, bool checkSelfLoot )
		{
			if ( from.AccessLevel > AccessLevel.Player || from.InRange( this.GetWorldLocation(), 2 ) )
			{
				bool selfLoot = ( checkSelfLoot && ( from == m_Owner ) );

				if ( selfLoot )
				{
					ArrayList items = new ArrayList( this.Items );

					bool gathered = false;
					bool didntFit = false;

					Container pack = from.Backpack;

					bool checkRobe = true;

					for ( int i = 0; !didntFit && i < items.Count; ++i )
					{
						Item item = (Item)items[i];
						Point3D loc = item.Location;

						if ( (item.Layer == Layer.Hair || item.Layer == Layer.FacialHair) || !item.Movable || !GetRestoreInfo( item, ref loc ) )
							continue;

						if ( checkRobe )
						{
							DeathRobe robe = from.FindItemOnLayer( Layer.OuterTorso ) as DeathRobe;

							if ( robe != null )
							{
								Map map = from.Map;

								if ( map != null && map != Map.Internal )
									robe.MoveToWorld( from.Location, map );
							}
						}

						if ( m_EquipItems.Contains( item ) && from.EquipItem( item ) )
						{
							gathered = true;
						}
						else if ( pack != null && pack.CheckHold( from, item, false, true ) )
						{
							item.Location = loc;
							pack.AddItem( item );
							gathered = true;
						}
						else
						{
							didntFit = true;
						}
					}

					if ( gathered && !didntFit )
					{
						SetFlag(CorpseFlag.Carved, true);

						if ( ItemID == 0x2006 )
						{
							ProcessDelta();
							SendRemovePacket();
							ItemID = Utility.Random( 0xECA, 9 ); // bone graphic
							Hue = 0;
							ProcessDelta();
						}

						from.PlaySound( 0x3E3 );
						from.SendLocalizedMessage( 1062471 ); // You quickly gather all of your belongings.
						return;
					}

					if ( gathered && didntFit )
						from.SendLocalizedMessage( 1062472 ); // You gather some of your belongings. The rest remain on the corpse.
				}

				if ( !CheckLoot( from ) )
					return;

				base.OnDoubleClick( from );
			}
			else
			{
				from.SendLocalizedMessage( 500446 ); // That is too far away.
				return;
			}
		}

		public override void OnDoubleClick( Mobile from )
		{
			Open( from, Core.AOS );
		}

		public override bool CheckContentDisplay( Mobile from )
		{
			return false;
		}

		public override bool DisplaysContent{ get{ return false; } }

		public override void OnSingleClick( Mobile from )
		{
			int hue = Notoriety.GetHue( NotorietyHandlers.CorpseNotoriety( from, this ) );

			if ( m_CorpseName != null )
				from.Send( new AsciiMessage( Serial, ItemID, MessageType.Label, hue, 3, "", m_CorpseName ) );
			else if ( ((Body)this.Amount).IsHuman || ( this.Owner != null && this.Owner.Player )  )
				from.Send( new AsciiMessage( Serial, ItemID, MessageType.Label, hue, 3, "", String.Format( "a corpse of {0}", Name ) ) );
			else
				from.Send( new AsciiMessage( Serial, ItemID, MessageType.Label, hue, 3, "", String.Format( "remains of {0}", Name ) ) );
		}

		public void Carve( Mobile from, Item item )
		{
			Mobile dead = m_Owner;

			if ( GetFlag(CorpseFlag.Carved) || dead == null )
			{
				from.SendLocalizedMessage( 500485 ); // You see nothing useful to carve from the corpse.
			}
			else if ( ((Body)Amount).IsHuman && ItemID == 0x2006 )
			{
				new Blood( 0x122D ).MoveToWorld( Location, Map );

				new Blood( 0x122D ).MoveToWorld( Location, Map ); 

				if ( m_Owner.AccessLevel == AccessLevel.Player )
				{
					new Torso( m_Owner ).MoveToWorld( Location, Map ); 
					new LeftLeg( m_Owner ).MoveToWorld( Location, Map ); 
					new LeftArm( m_Owner ).MoveToWorld( Location, Map ); 
					new RightLeg( m_Owner ).MoveToWorld( Location, Map ); 
					new RightArm( m_Owner ).MoveToWorld( Location, Map ); 
					new Head( m_Owner ).MoveToWorld( Location, Map ); 
				}

                SetFlag(CorpseFlag.Carved, true);

				while ( Items.Count > 0 )
				{
					Item j = (Item)Items[0];

					if ( (j.Layer == Layer.Hair || j.Layer == Layer.FacialHair) || !j.Movable )
						j.Delete();
					else
						j.MoveToWorld( GetWorldLocation(), Map );
				}

				if ( IsCriminalAction( from ) )
					from.CriminalAction( true );

				Delete();
			}
			else if ( dead is BaseCreature )
			{
				((BaseCreature)dead).OnCarve( from, this );
			}
			else
			{
				from.SendLocalizedMessage( 500485 ); // You see nothing useful to carve from the corpse.
			}
		}
	}
}