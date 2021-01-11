using System;
using System.IO;
using Server;
using Server.Network;
using Server.Mobiles;

namespace Server.Gumps
{
	public class UpdatesGump
	{
		private const bool SendAdMessage = false;
		private static TimeSpan Ad_Time = TimeSpan.FromHours( 12 );
		private const string Ad_Prompt = "UOGamers: Rebirth is offered to you free of charge, in exchange for this we ask that you visit our website periodically and click the ads there.  Money from these ads goes to pay for the server which hosts this shard.  Would you like to go there now? (www.uogamers.net)";
		private const string Ad_Url = "http://www.uogamers.net/";
		private const string Ad_OkMsg = "Thank you for supporting UOGamers: Rebirth!";
		private const string Ad_CancelMsg = "In the future, please take time to click the ads.  The ads pay for bandwidth & hosting fees, without the money generated by YOU clicking the ads, the shard could not say up!";

		private static DateTime m_FileTime;
		private static Packet m_Packet;

		public static void Initialize()
		{
			EventSink.Login += new LoginEventHandler( On_Login );

			//Server.Commands.CommandSystem.Register( "updates", AccessLevel.Player, new Server.Commands.CommandEventHandler( ShowUpdates ) );
		}

		public static void On_Login( LoginEventArgs args )
		{
			PlayerMobile pm = args.Mobile as PlayerMobile;
			if ( pm == null )
				return;
			
			SendUpdateMsg( (PlayerMobile)args.Mobile );

			if ( SendAdMessage && pm.LastLogin+Ad_Time < DateTime.Now && pm.LastLogin != DateTime.MinValue )
				pm.SendGump( new WarningGump( 1060637, 30720, Ad_Prompt, 0xFFFFFF, 320, 240, new WarningGumpCallback( OpenBrowser_Callback ), null ) );
			pm.LastLogin = DateTime.Now;
		}

		private static void OpenBrowser_Callback( Mobile m, bool okay, object unused )
		{
			if ( okay )
			{
				m.SendAsciiMessage( Ad_OkMsg );
				m.LaunchBrowser( Ad_Url );
			}
			else
			{
				m.SendAsciiMessage( Ad_CancelMsg );
			}
		}

		public static void ShowUpdates( Mobile m )
		{
			PlayerMobile pm = m as PlayerMobile;
			if ( pm == null )
				return;
			pm.LastUpdate = 0;
			SendUpdateMsg( pm );
		}

		private static void SendUpdateMsg( PlayerMobile m )
		{
			if ( !File.Exists( "update.txt" ) )
				return;

			DateTime time = File.GetLastWriteTimeUtc( "update.txt" );
			int ticks = (int)time.Ticks;
			if ( m.LastUpdate == ticks )
				return;

			if ( time != m_FileTime || m_Packet == null )
			{
				m_FileTime = time;
				Packet.Release( ref m_Packet );
				m_Packet = new ScrollMessage( 0x02, ticks, ReadFile( "update.txt" ) );
				m_Packet.SetStatic();
			}

			m.Send( m_Packet );
			m.LastUpdate = ticks;
		}

		private static string ReadFile( string name )
		{
			using ( StreamReader reader = new StreamReader( name ) )
				return reader.ReadToEnd().Replace( (char)10, (char)13 ) ;
		}
	}
}
