using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto
{
    public class TreeChallenge
    {
        public string Description { get; set; }
        public Func<Player, bool> Condition { get; set; }

        
    }
    public class ChallengeManager
    {
        private Random rand = new Random();

        public List<TreeChallenge> GetChallengesFor(string treeType)
        {
            var challenges = new List<TreeChallenge>();

            if (treeType == "BTree")
            {
                var possible = new List<TreeChallenge>
            {
                new TreeChallenge { Description = "Consigue 4 nodos", Condition = p => p.Tree != null && p.Tree.CountNodes() >= 4 },
                new TreeChallenge { Description = "Inserta 10 claves", Condition = p => p.TotalInsertedKeys >= 10 },
                new TreeChallenge { Description = "Obtén una altura de 3", Condition = p => p.Tree != null && p.Tree.Depth() >= 3 }
            };
                return possible.OrderBy(x => rand.Next()).Take(3).ToList();
            }

            if (treeType == "BST")
            {
                var possible = new List<TreeChallenge>
            {
                new TreeChallenge { Description = "Consigue 5 nodos", Condition = p => p.BSTree != null && p.BSTree.CountNodes() >= 5 },
                new TreeChallenge { Description = "Inserta 8 números", Condition = p => p.TotalInsertedKeys >= 8 },
                new TreeChallenge { Description = "Ten 3 nodos con hijos", Condition = p => p.BSTree != null && CountNodesWithChildren(p.BSTree.Root) >= 3 }
            };
                return possible.OrderBy(x => rand.Next()).Take(3).ToList();
            }

            if (treeType == "AVL")
            {
                var possible = new List<TreeChallenge>
            {
                new TreeChallenge { Description = "Rota 3 veces el árbol", Condition = p => p.AVLRotations >= 3 },
                new TreeChallenge { Description = "Altura mínima 4", Condition = p => p.AVLTree != null && p.AVLTree.Depth() >= 4 },
                new TreeChallenge { Description = "Consigue 6 nodos", Condition = p => p.AVLTree != null && p.AVLTree.CountNodes() >= 6 }
            };
                return possible.OrderBy(x => rand.Next()).Take(3).ToList();
            }

            return challenges;
        }

        private int CountNodesWithChildren(BSTNode node)
        {
            if (node == null) return 0;
            int count = 0;
            if (node.Left != null || node.Right != null) count++;
            count += CountNodesWithChildren(node.Left);
            count += CountNodesWithChildren(node.Right);
            return count;
        }

        

    }

}
