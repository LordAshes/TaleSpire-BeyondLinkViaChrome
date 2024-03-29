﻿using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

namespace LordAshes
{
    class BeyondLinkServer
    {
        static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        static private string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        static string data = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"/CustomData/";

        static Dictionary<string, object> stats = new Dictionary<string, object>();

        static System.Timers.Timer pulse = new System.Timers.Timer(5000);

        static bool learn = false;

        static void Main(string[] args)
        {
            //
            // Plugin Mode: No parameters means the server was launched by the TaleSpire plugin
            //
            if (!Environment.CommandLine.ToUpper().Contains("LEARN"))
            {
                // Check to see that TaleSpire is running
                pulse.Elapsed += (s, e) =>
                {
                    Process[] pname = Process.GetProcessesByName("TaleSpire");
                    if (pname.Length == 0)
                    {
                        // If TaleSpire is not running, end execution.
                        Environment.Exit(0);
                    }
                };
                pulse.Start();
            }
            else
            {
                learn = true;
            }
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, int.Parse(args[0])));
            serverSocket.Listen(1); //just one socket
            serverSocket.BeginAccept(null, 0, OnAccept, null);
            Console.Read();
        }

        private static void OnAccept(IAsyncResult result)
        {
            byte[] buffer = new byte[1024 * 1024];
            try
            {
                Socket client = null;
                string headerResponse = "";
                if (serverSocket != null && serverSocket.IsBound)
                {
                    client = serverSocket.EndAccept(result);
                    var i = client.Receive(buffer);
                    headerResponse = (System.Text.Encoding.UTF8.GetString(buffer)).Substring(0, i);
                }
                if (client != null)
                {
                    /* Handshaking and managing ClientSocket */
                    var key = headerResponse.Replace("ey:", "`")
                              .Split('`')[1]                     // dGhlIHNhbXBsZSBub25jZQ== \r\n .......
                              .Replace("\r", "").Split('\n')[0]  // dGhlIHNhbXBsZSBub25jZQ==
                              .Trim();

                    // key should now equal dGhlIHNhbXBsZSBub25jZQ==
                    var test1 = AcceptKey(ref key);

                    var newLine = "\r\n";

                    var response = "HTTP/1.1 101 Switching Protocols" + newLine
                         + "Upgrade: websocket" + newLine
                         + "Connection: Upgrade" + newLine
                         + "Sec-WebSocket-Accept: " + test1 + newLine + newLine
                         //+ "Sec-WebSocket-Protocol: chat, superchat" + newLine
                         //+ "Sec-WebSocket-Version: 13" + newLine
                         ;

                    client.Send(System.Text.Encoding.UTF8.GetBytes(response));

                    var i = client.Receive(buffer);
                    string browserSent = GetDecodedData(buffer, i);
                    // Console.WriteLine(browserSent);

                    if (learn) { System.IO.File.WriteAllText(data + @"\Content.json", browserSent); }

                    Dictionary<string, dynamic> message = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(browserSent);

                    Console.WriteLine(message["Name"] + " @ " + DateTime.UtcNow);

                    foreach (KeyValuePair<string, dynamic> item in message)
                    {
                        ProcessElement(message["Name"], item);
                    }

                    client.Close();
                }
            }
            catch (SocketException)
            {
            }
            finally
            {
                if (serverSocket != null && serverSocket.IsBound)
                {
                    serverSocket.BeginAccept(null, 0, OnAccept, null);
                }
            }
        }

        public static void ProcessElement(string name, KeyValuePair<string, dynamic> element, string root = "")
        {
            // Resurse through the hierarchy
            if (element.Value.GetType().ToString().EndsWith("JObject"))
            {
                // JObjects are sub-level dictionaries
                foreach (KeyValuePair<string, dynamic> item in JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(element.Value.ToString()))
                {
                    ProcessElement(name, item, (root == "") ? element.Key : root + "." + element.Key);
                }
            }
            else
            {
                if (element.Value.ToString() != "")
                {
                    string key = name + "." + ((root == "") ? element.Key : root + "." + element.Key);
                    bool requiresUpdate = true;
                    if (stats.ContainsKey(key))
                    {
                        // Old stat - check for change
                        if (stats[key].ToString() == element.Value.ToString())
                        {
                            // No change - avoid update
                            requiresUpdate = false;
                        }
                        else
                        {
                            // Change - remoive key (the updated key will be added below)
                            Console.WriteLine(key + " : Changed To " + element.Value.ToString());
                            stats.Remove(key);
                        }
                    }
                    else
                    {
                        // New entry
                        Console.WriteLine(key + " : New With " + element.Value.ToString());
                    }
                    if (requiresUpdate)
                    {
                        // Change to any stat which is used for rolls causes DSM (DiceSelectionMacro) file update
                        stats.Add(key, element.Value);
                        if (key.StartsWith(name + ".Saves.") ||
                           key.StartsWith(name + ".Init") ||
                           key.StartsWith(name + ".Attacks.") ||
                           key.StartsWith(name + ".Skills.") ||
                           key.StartsWith(name + ".Abilities.") ||
                           key.StartsWith(name + ".Prof"))
                        {
                            System.IO.File.WriteAllText(data + name + ".dsm", "");
                            foreach (KeyValuePair<string, object> stat in stats)
                            {
                                if (stat.Key.StartsWith(name + ".Init"))
                                {
                                    System.IO.File.AppendAllText(data + name + ".dsm", "Initiative [1D20" + stat.Value.ToString() + "]\r\n");
                                }
                            }
                            foreach (KeyValuePair<string, object> stat in stats)
                            {
                                if (stat.Key.StartsWith(name + ".Attacks.") && stat.Key.Contains("damage.amount"))
                                {
                                    string rootKey = stat.Key.Replace(".damage.amount", "");
                                    System.IO.File.AppendAllText(data + name + ".dsm", rootKey.Substring(rootKey.LastIndexOf(".") + 1) + " [1D20" + stats[rootKey].ToString() + "/" + stats[rootKey + ".damage.type"] + ":" + stats[rootKey + ".damage.amount"] + "]\r\n");
                                }
                            }
                            foreach (KeyValuePair<string, object> stat in stats)
                            {
                                if (stat.Key.StartsWith(name + ".Saves."))
                                {
                                    System.IO.File.AppendAllText(data + name + ".dsm", stat.Key.Substring(stat.Key.LastIndexOf(".") + 1) + " Save [1D20" + stat.Value.ToString() + "]\r\n");
                                }
                            }
                            foreach (KeyValuePair<string, object> stat in stats)
                            {
                                if (stat.Key.StartsWith(name + ".Skills."))
                                {
                                    System.IO.File.AppendAllText(data + name + ".dsm", stat.Key.Substring(stat.Key.LastIndexOf(".") + 1) + " [1D20" + stat.Value.ToString() + "]\r\n");
                                }
                            }
                            foreach (KeyValuePair<string, object> stat in stats)
                            {
                                if (stat.Key.StartsWith(name + ".Abilities."))
                                {
                                    System.IO.File.AppendAllText(data + name + ".dsm", stat.Key.Substring(stat.Key.LastIndexOf(".") + 1) + " [1D20" + stat.Value.ToString() + "]\r\n");
                                }
                            }
                            foreach (KeyValuePair<string, object> stat in stats)
                            {
                                if (stat.Key.StartsWith(name + ".Prof"))
                                {
                                    System.IO.File.AppendAllText(data + name + ".dsm", "Proficiency [1D20" + stat.Value.ToString() + "]\r\n");
                                }
                            }
                            System.IO.File.AppendAllText(data + name + ".dsm", "Custom [?]\r\n");
                        }
                        else
                        {
                            // Other stats are created as individual files
                            System.IO.File.WriteAllText(data + key, element.Value.ToString());
                        }
                    }
                }
            }
        }

        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        private static string AcceptKey(ref string key)
        {
            string longKey = key + guid;
            byte[] hashBytes = ComputeHash(longKey);
            return Convert.ToBase64String(hashBytes);
        }

        static SHA1 sha1 = SHA1CryptoServiceProvider.Create();
        private static byte[] ComputeHash(string str)
        {
            return sha1.ComputeHash(System.Text.Encoding.ASCII.GetBytes(str));
        }

        //Needed to decode frame
        public static string GetDecodedData(byte[] buffer, int length)
        {
            byte b = buffer[1];
            int dataLength = 0;
            int totalLength = 0;
            int keyIndex = 0;

            if (b - 128 <= 125)
            {
                dataLength = b - 128;
                keyIndex = 2;
                totalLength = dataLength + 6;
            }

            if (b - 128 == 126)
            {
                dataLength = BitConverter.ToInt16(new byte[] { buffer[3], buffer[2] }, 0);
                keyIndex = 4;
                totalLength = dataLength + 8;
            }

            if (b - 128 == 127)
            {
                dataLength = (int)BitConverter.ToInt64(new byte[] { buffer[9], buffer[8], buffer[7], buffer[6], buffer[5], buffer[4], buffer[3], buffer[2] }, 0);
                keyIndex = 10;
                totalLength = dataLength + 14;
            }

            if (totalLength > length)
            {
                // throw new Exception("The buffer length is small than the data length");
                buffer = new byte[totalLength];
                length = totalLength;
            }

            byte[] key = new byte[] { buffer[keyIndex], buffer[keyIndex + 1], buffer[keyIndex + 2], buffer[keyIndex + 3] };

            int dataIndex = keyIndex + 4;
            int count = 0;
            for (int i = dataIndex; i < totalLength; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ key[count % 4]);
                count++;
            }

            return Encoding.ASCII.GetString(buffer, dataIndex, dataLength);
        }

        //function to create  frames to send to client 
        /// <summary>
        /// Enum for opcode types
        /// </summary>
        public enum EOpcodeType
        {
            /* Denotes a continuation code */
            Fragment = 0,

            /* Denotes a text code */
            Text = 1,

            /* Denotes a binary code */
            Binary = 2,

            /* Denotes a closed connection */
            ClosedConnection = 8,

            /* Denotes a ping*/
            Ping = 9,

            /* Denotes a pong */
            Pong = 10
        }

        /// <summary>Gets an encoded websocket frame to send to a client from a string</summary>
        /// <param name="Message">The message to encode into the frame</param>
        /// <param name="Opcode">The opcode of the frame</param>
        /// <returns>Byte array in form of a websocket frame</returns>
        public static byte[] GetFrameFromString(string Message, EOpcodeType Opcode = EOpcodeType.Text)
        {
            byte[] response;
            byte[] bytesRaw = Encoding.Default.GetBytes(Message);
            byte[] frame = new byte[10];

            long indexStartRawData = -1;
            long length = (long)bytesRaw.Length;

            frame[0] = (byte)(128 + (int)Opcode);
            if (length <= 125)
            {
                frame[1] = (byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = (byte)126;
                frame[2] = (byte)((length >> 8) & 255);
                frame[3] = (byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = (byte)127;
                frame[2] = (byte)((length >> 56) & 255);
                frame[3] = (byte)((length >> 48) & 255);
                frame[4] = (byte)((length >> 40) & 255);
                frame[5] = (byte)((length >> 32) & 255);
                frame[6] = (byte)((length >> 24) & 255);
                frame[7] = (byte)((length >> 16) & 255);
                frame[8] = (byte)((length >> 8) & 255);
                frame[9] = (byte)(length & 255);

                indexStartRawData = 10;
            }

            response = new byte[indexStartRawData + length];

            long i, reponseIdx = 0;

            //Add the frame bytes to the reponse
            for (i = 0; i < indexStartRawData; i++)
            {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            //Add the data bytes to the response
            for (i = 0; i < length; i++)
            {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            return response;
        }
    }
}