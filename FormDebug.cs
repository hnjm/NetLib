using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    public partial class FormDebug : Form {
        readonly TreeNodeCollection clientsAll;
        readonly TreeNodeCollection clientsOff;
        readonly TreeNodeCollection clientsOn;
        readonly Dictionary<SGuid, string> nicks = new Dictionary<SGuid, string>();
        readonly TreeNode nodeClients;
        readonly TreeNode nodeClientsAll;
        readonly TreeNode nodeClientsOff;
        readonly TreeNode nodeClientsOn;
        readonly List<DebugNode> nodes = new List<DebugNode>();
        readonly TreeNode nodeServers;
        readonly TreeNodeCollection servers;
        bool closed;
        DebugNode lastSelected;

        public FormDebug() {
            InitializeComponent();
            nodeServers = treeView.Nodes["servers"];
            nodeClients = treeView.Nodes["clients"];
            nodeClientsOn = nodeClients.Nodes["clientsOn"];
            nodeClientsOff = nodeClients.Nodes["clientsOff"];
            nodeClientsAll = nodeClients.Nodes["clientsAll"];
            servers = nodeServers.Nodes;
            clientsOn = nodeClientsOn.Nodes;
            clientsOff = nodeClientsOff.Nodes;
            clientsAll = nodeClientsAll.Nodes;
            FormClosing += (a, b) => closed = true;
            cbRaw.Checked = trackRaw;
            cbCommands.Checked = trackCommands;
            cbDirectives.Checked = trackHigh;
            cbRaw.CheckedChanged += (a, b) => trackRaw = cbRaw.Checked;
            cbCommands.CheckedChanged += (a, b) => trackCommands = cbCommands.Checked;
            cbDirectives.CheckedChanged += (a, b) => trackHigh = cbDirectives.Checked;
            debugInstances.added += a => {
                if (!closed) Invoke(new Action<Identified>(objectAdded), a);
            };
            debugInstances.removed += a => {
                if (!closed) Invoke(new Action<Identified>(objectRemoved), a);
            };
            dbgNotice += a => {
                if (!closed) Invoke(new Action<Identified>(notice), a);
            };
            treeView.NodeMouseClick += refreshAll;
            tableRaw.RowEnter += showRawMsg;
            tableHigh.RowEnter += showHighMsg;
        }
        void objectAdded(Identified obj) {
            var dn = new DebugNode(obj, nicks.ContainsKey(obj.ID) ? nicks[obj.ID] : null);
            nodes.Add(dn);
            if (obj is Client) {
                if (dn.bindClient.Connected)
                    clientsOn.Add(dn);
                else
                    clientsOff.Add(dn);
                DebugNode dna = dn.Clone();
                nodes.Add(dna);
                clientsAll.Add(dna);
            }
            else if (obj is Server) {
                servers.Add(dn);
                foreach (Link l in dn.bindServer.clients) {
                    var ldn = new DebugNode(l, nicks.ContainsKey(obj.ID) ? nicks[obj.ID] : null);
                    nodes.Add(ldn);
                    dn.Nodes.Add(ldn);
                }
            }

            treeView.ExpandAll();
        }
        void objectRemoved(Identified obj) {
            List<DebugNode> dns = nodes.FindAll(a => a.ID == obj.ID);
            foreach (DebugNode dn in dns) {
                if (cbRmUnused.Checked) {
                    nodes.Remove(dn);
                    if (obj is Client) {
                        clientsOn.Remove(dn);
                        clientsOff.Remove(dn);
                        clientsAll.Remove(dn);
                    } else if (obj is Server)
                        servers.Remove(dn);
                    if (nicks.ContainsKey(obj.ID))
                        nicks.Remove(obj.ID);
                }
                dn.NodeFont = new Font(dn.NodeFont ?? treeView.Font, FontStyle.Strikeout);
            }

            treeView.ExpandAll();
        }
        void notice(Identified obj) {
            if (obj is User u) {
                if (nicks.ContainsKey(u.ID))
                    nicks[u.ID] = u.Nickname;
                else
                    nicks.Add(u.ID, u.Nickname);
                foreach (DebugNode n in nodes)
                    if (n.ID == u.ID)
                        n.Text = u.Nickname;
            }
            else if (obj is Link) {
                foreach (DebugNode node in nodes.ToList())
                    if (node.bind == obj) {
                        DebugNode s = nodes.Find(a => a.Nodes.Contains(node));
                        if (s != null)
                            s.Nodes.Remove(node);
                        nodes.Remove(node);
                        nicks.Remove(node.ID);
                    }
            }
            else if (obj is Client c) {
                DebugNode n = nodes.Find(a => a.ID == c.ID);
                if (n != null)
                    if (c.Connected) {
                        clientsOff.Remove(n);
                        clientsOn.Add(n);
                    }
                    else {
                        clientsOn.Remove(n);
                        clientsOff.Add(n);
                    }
            }
            else if (obj is Server s) {
                DebugNode sN = nodes.Find(a => a.ID == s.ID);
                var subN = new DebugNode[sN.Nodes.Count];
                sN.Nodes.CopyTo(subN, 0);
                List<DebugNode> subL = subN.ToList();
                foreach (DebugNode sL in subN)
                    if (!s.clients.Contains(sL.bindLink))
                        sN.Nodes.Remove(sL);
                foreach (Link l in s.clients)
                    if (!subL.Exists(a => a.bindLink == l)) {
                        var ldn = new DebugNode(l, nicks.ContainsKey(obj.ID) ? nicks[obj.ID] : null);
                        nodes.Add(ldn);
                        sN.Nodes.Add(ldn);
                    }
            }

            treeView.ExpandAll();
        }
        void refreshAll(object sender, TreeNodeMouseClickEventArgs e) {
            if (e.Node is DebugNode dn && nodes.Exists(a => a.ID == dn.ID)) {
                if (lastSelected != null)
                    lastSelected.bind.clearEvents();
                lastSelected = dn;
                Identified b = dn.bind;
                b.tRaw.added += a => addRaw(b, a);
                b.tCommand.added += a => addCommand(b, a);
                b.tHigh.added += a => addHigh(b, a);
                tableRawMsg.Rows.Clear();
                tableRaw.Rows.Clear();
                tableCommands.Rows.Clear();
                tableHigh.Rows.Clear();
                foreach (object[] row in b.tRaw.ToList())
                    addRaw(null, row);
                foreach (object[] row in b.tCommand.ToList())
                    addCommand(null, row);
                foreach (object[] row in b.tHigh.ToList())
                    addHigh(null, row);
            }
        }
        void addRaw(Identified id, params object[] row) {
            if (!closed)
                Invoke(new Action(() => {
                    if (id == null || lastSelected?.bind == id)
                        tableRaw.Rows.Add(
                            (from cell in row
                                where !(cell is string[])
                                select cell.ToString()).Concat(new[] {((string[]) row[5]).Length + ""}).ToArray());
                }));
        }
        void addCommand(Identified id, params object[] row) {
            if (!closed)
                Invoke(new Action(() => {
                    if (id == null || lastSelected?.bind == id)
                        tableCommands.Rows.Add(row.Select(c => {
                            if (c is object[]) {
                                string[] strs = ((object[]) c).AllStrings();
                                return $"{strs[0]}({string.Join(", ", strs.Skip(1))})";
                            }

                            return c;
                        }).ToArray());
                }));
        }
        void addHigh(Identified id, params object[] row) {
            if (!closed)
                Invoke(new Action(() => {
                    if (id == null || lastSelected?.bind == id) {
                        object col3 = "",
                            col4 = "",
                            col5 = "";
                        if (row.Length == 3) {
                            if (row[2] is Directive dir) {
                                col3 = dir.type + "";
                                col4 = $"{dir.from} → {(dir.to == SGuid.Empty ? "Everyone" : dir.to + "")}";
                                col5 = dir.values.Length;
                            }
                        }
                        else {
                            col4 = row[2];
                            col5 = row[3];
                        }
                        tableHigh.Rows.Add(row[0], row[1], col3, col4, col5);
                    }
                }));
        }
        void showRawMsg(object sender, DataGridViewCellEventArgs e) {
            if (lastSelected != null) {
                tableRawMsg.Rows.Clear();
                object[] row = lastSelected.bind.tRaw[e.RowIndex];
                var intern = (bool) row[4];
                var cmds = (string[]) row[5];
                var wasDisconnect = false;
                for (var i = 0; i < cmds.Length; i++) {
                    bool reallyIntern = intern && int.TryParse(cmds[i], out _);
                    object col3;
                    if (reallyIntern)
                        if (wasDisconnect) {
                            wasDisconnect = false;
                            col3 = StringEnum<StopReason>(cmds[i]);
                        }
                        else {
                            var cmd = StringEnum<InternalCommands>(cmds[i]);
                            wasDisconnect = i == 0 && cmd == InternalCommands.disconnect;
                            col3 = cmd;
                        }
                    else if (intern)
                        col3 = cmds[i];
                    else
                        col3 = string.Join(Directive.textSep_s, cmds[i].Split(Directive.textSep_c).Select((a, j) =>
                            j == 0 ? StringEnum<ManagedCommands>(a).ToString() : j > 2 ? Data.FromBase64(a) : a
                        ));
                    tableRawMsg.Rows.Add(i, cmds[i].Length, col3);
                }
            }
        }
        void showHighMsg(object sender, DataGridViewCellEventArgs e) {
            if (lastSelected != null) {
                tableHighMsg.Rows.Clear();
                object[] row = lastSelected.bind.tHigh[e.RowIndex];
                if (row.Length == 3 && row[2] is Directive dir)
                    for (var i = 0; i < dir.values.Length; i++)
                        tableHighMsg.Rows.Add(i,
                            i == 0 && dir.type == ManagedCommands.leave
                                ? StringEnum<StopReason>(dir.values[i]).ToString()
                                : dir.values[i]);
            }
        }

        public class DebugNode : TreeNode {
            public readonly Identified bind;
            string _text;
            public bool isClient, isServer, isLink;
            public DebugNode(Identified obj, string text = null) {
                bind = obj;
                isClient = obj is Client;
                isServer = obj is Server;
                isLink = obj is Link;
                Text = text;
            }
            public Client bindClient => isClient ? (Client) bind : throw new InvalidOperationException();
            public Server bindServer => isServer ? (Server) bind : throw new InvalidOperationException();
            public Link bindLink => isLink ? (Link) bind : throw new InvalidOperationException();
            public SGuid ID => bind.ID;
            public new string Text {
                get => _text;
                set {
                    _text = value;
                    base.Text = value != null ? $"{value} @{ID}" : ID + "";
                }
            }
            public new string ToolTipText => Text;
            public new DebugNode Clone() => new DebugNode(bind, _text);
        }
    }
}