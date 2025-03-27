Public Class Form1

#Region "Variables Globales"
    Dim goLeft, goRight As Boolean ' Indique si le joueur se déplace vers la gauche ou la droite
    Dim speed As Integer = 8 ' Vitesse de déplacement du joueur
    Dim score As Integer = 0 ' Score du joueur
    Dim missed As Integer = 0 ' Nombre d'œufs manqués par le joueur
    Dim level As Integer = 1 ' Niveau actuel du jeu
    Dim randX As New Random() ' Générateur de nombres aléatoires pour la position en X
    Dim randY As New Random() ' Générateur de nombres aléatoires pour la position en Y
    Dim splash As New PictureBox() ' Image affichée lorsqu'un œuf touche le sol
    Dim bonus As New PictureBox() ' Image du bonus lorsqu'il apparaît
    Dim isPaused As Boolean = False ' Indique si le jeu est en pause
    Dim highScores As New List(Of Integer)() ' Liste des meilleurs scores
    ' Objets pour gérer les sons du jeu
    Dim soundPlayerMove As New System.Media.SoundPlayer("C:\Users\code x maker\Documents\Visual Studio 2015\Projects\SauverLesOeufs\SauverLesOeufs\Resources\falling.wav") ' Son de déplacement
    Dim soundPlayerMissed As New System.Media.SoundPlayer("C:\Users\code x maker\Documents\Visual Studio 2015\Projects\SauverLesOeufs\SauverLesOeufs\Resources\eggcrack.wav") ' Son lorsque l'œuf est manqué
    Dim soundPlayerCaught As New System.Media.SoundPlayer("C:\Users\code x maker\Documents\Visual Studio 2015\Projects\SauverLesOeufs\SauverLesOeufs\Resources\ball.wav") ' Son lorsque l'œuf est attrapé
    Dim audioFilesLoaded As Boolean = True ' Indique si les fichiers audio sont correctement chargés
    Dim bonusActive As Boolean = False ' Indique si le bonus est actuellement actif
    Dim bonusTimer As New Timer() ' Timer pour gérer la durée du bonus
    Dim eggsCaughtSinceLastBonus As Integer = 0 ' Compteur d'œufs attrapés depuis le dernier bonus
    Const EggsNeededForBonus As Integer = 15 ' Nombre d'œufs nécessaires pour faire apparaître le bonus
#End Region


#Region "Gestion du Jeu"
    Private Sub gameTick(sender As Object, e As EventArgs) Handles GameTimer.Tick
        If isPaused Then Return ' Si le jeu est en pause, ne rien faire

        ' Mettre à jour les informations affichées
        txtScore.Text = "Sauvés: " & score
        txtMiss.Text = "Manqués: " & missed
        txtLevel.Text = "Niveau: " & level

        ' Déplacement du poulet
        If goLeft AndAlso player.Left > 0 Then
            player.Left -= speed
            player.Image = My.Resources.chicken_normal2
            If audioFilesLoaded Then PlaySound("move")
        End If
        If goRight AndAlso player.Left + player.Width < Me.ClientSize.Width Then
            player.Left += speed
            player.Image = My.Resources.chicken_normal
            If audioFilesLoaded Then PlaySound("move")
        End If

        ' Gestion des œufs qui tombent
        For Each x As Control In Me.Controls
            If TypeOf x Is PictureBox AndAlso CStr(x.Tag) = "eggs" Then
                x.Top += speed ' Faire tomber l'œuf
                ' Si l'œuf atteint le bas de l'écran
                If x.Top + x.Height > Me.ClientSize.Height Then
                    GérerImpact(x) ' Gérer l'impact (œuf manqué)
                End If
                ' Si le poulet attrape l'œuf
                If player.Bounds.IntersectsWith(x.Bounds) Then
                    AttraperOeuf(x) ' Gérer la capture de l'œuf
                End If
            End If
        Next

        ' Gestion du bonus qui tombe
        If bonusActive Then
            bonus.Top += speed ' Faire tomber le bonus avec la même vitesse que les œufs

            ' Si le bonus atteint le bas de l'écran, le supprimer
            If bonus.Top + bonus.Height > Me.ClientSize.Height Then
                DisparaitreBonus()
            End If

            ' Vérifier si le poulet attrape le bonus
            If player.Bounds.IntersectsWith(bonus.Bounds) Then
                AttraperBonus()
            End If
        End If

        ' Apparition du bonus après avoir attrapé 15 œufs
        If Not bonusActive AndAlso eggsCaughtSinceLastBonus >= EggsNeededForBonus Then
            AfficherBonus()
            eggsCaughtSinceLastBonus = 0 ' Réinitialiser le compteur
        End If

        ' Passage au niveau supérieur
        If score >= level * 10 Then
            level += 1
            speed += 2 ' Augmenter la vitesse des œufs
        End If

        ' Vérifier si le jeu est terminé (trop d'œufs manqués)
        If missed > 5 Then
            GameTimer.Stop() ' Arrêter le jeu
            EnregistrerScore(score) ' Enregistrer le score
            AfficherMeilleursScores() ' Afficher les meilleurs scores
            MessageBox.Show("Game Over mon ami(e)!" & Environment.NewLine & "Vous avez perdu de bons œufs!" & Environment.NewLine & "Cliquez sur OK pour réessayer")
            RestartGame() ' Redémarrer le jeu
        End If
    End Sub

    Private Sub GérerImpact(oeuf As Control)
        splash.Image = My.Resources.splash
        splash.Location = oeuf.Location
        splash.Height = 60
        splash.Width = 60
        splash.BackColor = Color.Transparent
        Me.Controls.Add(splash)

        oeuf.Top = randY.Next(80, 300) * -1
        oeuf.Left = randX.Next(5, Me.ClientSize.Width - oeuf.Width)
        missed += 1
        player.Image = My.Resources.chicken_hurt
        If audioFilesLoaded Then PlaySound("missed")
    End Sub

    Private Sub AttraperOeuf(oeuf As Control)
        oeuf.Top = randY.Next(80, 300) * -1
        oeuf.Left = randX.Next(5, Me.ClientSize.Width - oeuf.Width)
        score += 1
        eggsCaughtSinceLastBonus += 1 ' Incrémenter le compteur d'œufs attrapés
        If audioFilesLoaded Then PlaySound("caught")
    End Sub
#End Region

#Region "Gestion du Bonus"
    Private Sub AfficherBonus()
        ' Vérifie si le bonus est déjà affiché
        If bonusActive Then Return

        ' Initialisation du PictureBox du bonus
        bonus = New PictureBox()
        bonus.Image = Image.FromFile("C:\Users\code x maker\Documents\Visual Studio 2015\Projects\SauverLesOeufs\SauverLesOeufs\Resources\bonus_icon.png")
        bonus.Tag = "bonus"
        bonus.SizeMode = PictureBoxSizeMode.StretchImage
        bonus.Height = 50
        bonus.Width = 50
        bonus.Left = randX.Next(5, Me.ClientSize.Width - bonus.Width)
        bonus.Top = -bonus.Height ' Positionné hors écran en haut pour qu'il tombe
        bonus.BackColor = Color.Transparent

        ' Ajouter le bonus au formulaire
        Me.Controls.Add(bonus)
        bonus.BringToFront() ' S'assurer que le bonus est au premier plan
        bonusActive = True

        ' Démarrer un timer pour faire disparaître le bonus après 5 secondes s'il n'est pas attrapé
        bonusTimer.Interval = 5000 ' 5 secondes
        AddHandler bonusTimer.Tick, AddressOf DisparaitreBonus
        bonusTimer.Start()
    End Sub

    Private Sub AttraperBonus()
        ' Appliquer l'effet du bonus : annuler 4 œufs manqués (en s'assurant que "missed" ne devienne pas négatif)
        missed = Math.Max(missed - 4, 0)
        ' Bonus optionnel de points
        score += 5
        DisparaitreBonus() ' Supprimer le bonus de l'écran
    End Sub

    Private Sub DisparaitreBonus()
        If bonusActive Then
            Me.Controls.Remove(bonus) ' Supprimer le bonus de l'écran
            bonusActive = False
            bonusTimer.Stop() ' Arrêter le timer
        End If
    End Sub
#End Region

#Region "Enregistrement et Affichage des Scores"
    Private Sub EnregistrerScore(nouveauScore As Integer)
        highScores.Add(nouveauScore)
        highScores = highScores.OrderByDescending(Function(s) s).Take(5).ToList()
    End Sub

    Private Sub AfficherMeilleursScores()
        Dim message As String = "Meilleurs Scores:" & Environment.NewLine
        For i As Integer = 0 To highScores.Count - 1
            message &= (i + 1) & ". " & highScores(i) & Environment.NewLine
        Next
        MessageBox.Show(message, "Meilleurs Scores")
    End Sub
#End Region

#Region "Gestion des Sons"
    Private Sub PlaySound(action As String)
        If Not audioFilesLoaded Then Return

        Try
            Select Case action
                Case "move"
                    soundPlayerMove.Play()
                Case "missed"
                    soundPlayerMissed.Play()
                Case "caught"
                    soundPlayerCaught.Play()
            End Select
        Catch ex As Exception
            ' Ignorer les erreurs de lecture des sons pour éviter de bloquer le jeu
        End Try
    End Sub
#End Region

#Region "Gestion des Entrées Clavier"
    Private Sub keyisdown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Left Then goLeft = True
        If e.KeyCode = Keys.Right Then goRight = True
        If e.KeyCode = Keys.P Then
            isPaused = Not isPaused
            If isPaused Then GameTimer.Stop() Else GameTimer.Start()
        End If
    End Sub

    Private Sub keyisup(sender As Object, e As KeyEventArgs) Handles MyBase.KeyUp
        If e.KeyCode = Keys.Left Then goLeft = False
        If e.KeyCode = Keys.Right Then goRight = False
    End Sub
#End Region

#Region "Initialisation et Redémarrage"
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            soundPlayerMove.Load()
            soundPlayerMissed.Load()
            soundPlayerCaught.Load()
        Catch ex As Exception
            audioFilesLoaded = False
            MessageBox.Show("Erreur lors du chargement des fichiers audio. Les sons ne seront pas disponibles pendant le jeu.")
        End Try
        RestartGame()
    End Sub

    Private Sub RestartGame()
        For Each x As Control In Me.Controls
            If TypeOf x Is PictureBox AndAlso CStr(x.Tag) = "eggs" Then
                x.Top = randY.Next(80, 300) * -1
                x.Left = randX.Next(5, Me.ClientSize.Width - x.Width)
            End If
        Next
        player.Left = Me.ClientSize.Width / 2
        player.Image = My.Resources.chicken_normal
        score = 0
        missed = 0
        level = 1
        speed = 8
        goLeft = False
        goRight = False
        eggsCaughtSinceLastBonus = 0 ' Réinitialiser le compteur d'œufs attrapés
        GameTimer.Start()
    End Sub
#End Region

End Class
