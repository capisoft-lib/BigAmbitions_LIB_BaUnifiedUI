[b]LIB BA Unified UI 1.0.0[/b]

Bibliothèque indépendante d’interface utilisateur partagée pour le développement de mods Big Ambitions.

LIB BA Unified UI n’ajoute aucun élément de gameplay à elle seule. Elle fournit un ensemble stable de composants et de services d’interface dans le style du jeu, réutilisables par d’autres mods.

Le même paquet 1.0.0 prend en charge Big Ambitions EA 0.11 et la version 1.0 expérimentale.

[b]Nouveauté de cette mise à jour : sélecteur de couleur natif[/b]

Les mods peuvent maintenant ajouter un réglage de couleur persistant directement dans Options > Mods. Le composant utilise le sélecteur HSV natif de Big Ambitions, affiche un aperçu et la valeur hexadécimale en direct, prend en charge Réinitialiser tout et avertit le mod une seule fois lorsque le joueur ferme le sélecteur avec une couleur modifiée.

[b]Services d’interface inclus[/b]

[list]
[*]Options de couleur persistantes dans Options > Mods utilisant le sélecteur HSV du jeu
[*]Aperçus de couleur, valeurs hexadécimales et handles de changement en direct
[*]Panneaux, en-têtes, boutons, listes, champs de recherche et fenêtres contextuelles dans le style du jeu
[*]Listes avec rails de défilement visibles et curseurs déplaçables
[*]Outils de mise en page fluides et réutilisables avec dimensionnement automatique des panneaux
[*]Fenêtres déplaçables avec comportement natif du curseur et mémorisation des positions
[*]Compatibilité avec les API de fenêtres déplaçables d’EA 0.11 et de la version 1.0 expérimentale
[*]Masquage automatique lorsque l’écran Options du jeu est ouvert
[*]Raccourcis clavier configurables avec détection des conflits portant sur la combinaison complète
[*]Assembly public et espace de noms stables pour l’intégration dans les mods
[/list]

[b]Installation[/b]

[list]
[*]Abonnez-vous à LIB BA Unified UI sur le Steam Workshop
[*]Activez-la dans le menu Mods de Big Ambitions
[*]Redémarrez le jeu après une mise à jour
[/list]

[b]Pour les développeurs de mods[/b]

Référencez l’assembly stable [code]LIB_BaUnifiedUI[/code] depuis votre asmdef. N’intégrez pas une copie privée de la DLL dans un autre paquet.

Le code source et la documentation d’intégration sont disponibles dans le [url=https://github.com/capisoft-lib/BigAmbitions_LIB_BaUnifiedUI]dépôt GitHub[/url].

Bibliothèque communautaire créée par capisoft-lib. Aucun lien avec Hovgaard Games.

[b]Soutenir le développeur ☕[/b]

Si cette bibliothèque a évité à vos mods de réinventer chaque bouton, vous pouvez soutenir son développement en m’offrant un café :

[url=https://buymeacoffee.com/capitaine]☕ M’offrir un café[/url]

L’interface reste unifiée sans café ; pour son développeur, c’est moins certain.
