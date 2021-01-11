// UOGamers:  Rebirth Patch 1.1  - 9-23-2005 - Jakob
/* Added file as part of patch 1.1
 * */

using System;
using System.Web.Mail;
using System.Threading;
using Server;

namespace Server.Misc
{
	public class Email
	{
		/* In order to support emailing, fill in EmailServer:
		 * Example:
		 *  public static readonly string EmailServer = "mail.domain.com";
		 * 
		 * If you want to add crash reporting emailing, fill in CrashAddresses:
		 * Example:
		 *  public static readonly string CrashAddresses = "first@email.here;second@email.here;third@email.here";
		 * 
		 * If you want to add speech log page emailing, fill in SpeechLogPageAddresses:
		 * Example:
		 *  public static readonly string SpeechLogPageAddresses = "first@email.here;second@email.here;third@email.here";
		 */

		public static readonly string EmailServer = null;

		public static readonly string CrashAddresses = null;
		public static readonly string SpeechLogPageAddresses = null;

		// Ken's email: ken@uorebirth.com
		// Kinto's email: kinto@uorebirth.com

		public static void Configure()
		{
			if ( EmailServer != null )
				SmtpMail.SmtpServer = EmailServer;
		}

		public static bool Send( MailMessage message )
		{
			try
			{
				SmtpMail.Send( message );
			}
			catch
			{
				return false;
			}

			return true;
		}

		public static void AsyncSend( MailMessage message )
		{
			ThreadPool.QueueUserWorkItem( new WaitCallback( SendCallback ), message );
		}

		private static void SendCallback( object state )
		{
			MailMessage message = (MailMessage) state;

			if ( Send( message ) )
				Console.WriteLine( "Sent e-mail '{0}' to '{1}'.", message.Subject, message.To );
			else
				Console.WriteLine( "Failure sending e-mail '{0}' to '{1}'.", message.Subject, message.To );
		}
	}
}