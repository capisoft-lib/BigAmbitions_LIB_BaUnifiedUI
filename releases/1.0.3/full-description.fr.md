[b]LIB BA Unified UI 1.0.3[/b]

Bibliothèque indépendante d'interface utilisateur partagée pour le développement de mods Big Ambitions.

LIB BA Unified UI n'ajoute aucun élément de gameplay à elle seule. Elle fournit aux autres mods des composants et services d'interface réutilisables.

[b]Nouveauté : une saisie de texte fiable[/b]

Les contrôles créés dynamiquement héritent maintenant du même calque d’interface que leur panneau parent. Lorsqu’un champ de recherche ou de texte BAUI est actif, les raccourcis du jeu comme B et M restent bloqués au lieu d’ouvrir le téléphone ou la carte. Les transitions de focus évitent aussi les sélections EventSystem imbriquées. Les API publiques et l’identité stable de l’assembly restent inchangées.

[b]Services d'interface inclus[/b]

[list]
[*]Interrupteurs et curseurs natifs clonés depuis les réglages du jeu
[*]Options de couleur persistantes dans Options > Mods avec le sélecteur HSV du jeu
[*]Panneaux, en-têtes, boutons, listes, champs de recherche et popups au style vanilla
[*]Listes réutilisables avec barres de défilement visibles et poignées déplaçables
[*]Constructeurs de mise en page fluides avec dimensionnement automatique
[*]Fenêtres déplaçables avec curseur natif et positions mémorisées
[*]Masquage automatique pendant l'affichage des options du jeu
[*]Raccourcis clavier configurables avec détection des conflits de combinaisons
[*]Assembly et espace de noms publics stables pour les intégrations
[/list]

[b]Installation[/b]

[list]
[*]Abonnez-vous à LIB BA Unified UI sur le Steam Workshop
[*]Activez-la dans le menu Mods de Big Ambitions
[*]Redémarrez le jeu après une mise à jour
[/list]

[b]Pour les développeurs de mods[/b]

Référencez l'assembly stable [code]LIB_BaUnifiedUI[/code] depuis votre asmdef. N'intégrez pas de copie privée de la DLL dans un autre paquet. Les intégrations existantes n'ont pas besoin d'être recompilées.

Le code source et la documentation sont disponibles dans le [url=https://github.com/capisoft-lib/BigAmbitions_LIB_BaUnifiedUI]dépôt GitHub[/url].

Bibliothèque communautaire par capisoft-lib. Non affiliée à Hovgaard Games.

[b]Soutenir le développeur ☕[/b]

[url=https://buymeacoffee.com/capitaine]☕ Offrez-moi un café[/url]
