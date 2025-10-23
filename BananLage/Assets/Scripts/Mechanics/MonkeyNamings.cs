using System.Text;
using UnityEngine;

namespace Mechanics
{
    public static class MonkeyNamings
    {
        private static string[] _maleNames = {
            "João","José","Antônio","Francisco","Carlos","Paulo","Pedro","Lucas","Luiz","Marcos",
            "Rafael","Gabriel","Daniel","Marcelo","Bruno","Eduardo","Rodrigo","Fernando","André","Fábio",
            "Diego","Alexandre","Sérgio","Vitor","Thiago","Gustavo","Leonardo","Roberto","Felipe","Rogério",
            "Vinícius","Márcio","Murilo","Samuel","Cláudio","Ricardo","Wagner","Caio","Rafael","Cristiano",
            "Gilberto","Renato","Jorge","Alan","Júlio","Adriano","Otávio","Evandro","Igor","Henrique"
        };

        
        private static string[] _femaleNames = {
            "Maria","Ana","Francisca","Antônia","Adriana","Juliana","Marcia","Fernanda","Patrícia","Aline",
            "Sandra","Camila","Amanda","Bruna","Carla","Beatriz","Letícia","Carolina","Gabriela","Renata",
            "Daniela","Larissa","Bianca","Tatiane","Tatiana","Cláudia","Raquel","Cíntia","Elaine","Andreia",
            "Sabrina","Natália","Cristina","Flávia","Érica","Michele","Vanessa","Fabiana","Luciana","Gisele",
            "Rosângela","Verônica","Érika","Simone","Helena","Isabela","Jéssica","Paula","Viviane","Silvia"
        };
        
        private static string[] _surnames1 = {
            "Silva","Santos","Oliveira","Souza","Pereira","Lima","Carvalho","Ferreira","Rodrigues","Almeida",
            "Costa","Gomes","Martins","Araújo","Melo","Barbosa","Ribeiro","Dias","Teixeira","Moreira",
            "Correia","Moura","Campos","Cardoso","Rocha","Nunes","Monteiro","Cavalcanti","Freitas","Macedo",
            "Vieira","Batista","Andrade","Farias","Barros","Sales","Duarte","Borges","Fonseca","Machado",
            "Rezende","Queiroz","Xavier","Magalhães","Pinto","Rangel","Tavares","Prado","Amaral","Mendonça"
        };
        
        private static string[] _surnames2 = {
            "Cunha","Paiva","Peixoto","Brandão","Assis","França","Neves","Novaes","Brito","Guimarães",
            "Siqueira","Coutinho","Sarmento","Sobrinho","Figueiredo","Mattos","Coelho","Valente","Gonçalves","Teles",
            "Reis","Serra","Porto","Bezerra","Lacerda","Marques","Caires","Quevedo","Peres","Santana",
            "Seabra","Matos","Pinheiro","Junqueira","Bastos","Barreto","Parente","Queiróz","Alencar","Drumond",
            "Pacheco","Camargo","Castro","Pedrosa","Lourenço","Prates","Severino","Pimenta","Sodré","Filgueiras"
        };

        public static string NewName(this MonkeyGender gender)
        {
            var name = (gender == MonkeyGender.M ? _maleNames : _femaleNames).RandomEntry();
            var surname = _surnames1.RandomEntry();
            var surname2 = Random.value < 0.5f ? " " + (Random.value < 0.5f ? _surnames1 : _surnames2).RandomEntry() : "";
            return $"{name} {surname}{surname2}";
        }
    }
}