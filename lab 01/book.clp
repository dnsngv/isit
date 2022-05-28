;;****************
;;* DEFFUNCTIONS *
;;****************

(deffunction ask-question (?question $?allowed-values)
  (printout t ?question)
  (bind ?answer (read))
  (if (lexemep ?answer)
    then (bind ?answer (lowcase ?answer))
  )
  (while (not (member$ ?answer ?allowed-values)) do
    (printout t ?question)
    (bind ?answer (read))
    (if (lexemep ?answer)
      then (bind ?answer (lowcase ?answer)))
  )
  ?answer
)

(deffunction ask-question-range (?question ?min ?max)
  (printout t ?question)
  (bind ?answer (read))
  (while (or (not (numberp ?answer)) (> ?answer ?max) (< ?answer ?min)) do
    (printout t ?question)
    (bind ?answer (read))
  )
  ?answer
)

;;;***************
;;;* QUERY RULES *
;;;***************

(defrule book "Книга"
  (not (favorite-book ?))
  (not (repair ?))
  =>
  (assert (favorite-book (ask-question-range
    "Choose your favorite book (1.brothers-karamazov/2.metamorphosis/3.homo-deus/4.god-as-illusion)? "
    1 4
  )))
)

(defrule writer "Писатель"
  (not (favorite-writer ?))
  (not (repair ?))
  =>
  (assert (favorite-writer (ask-question
    "Choose your favorite writer (orwell/strugatsky/freud/gegel)? "
    orwell strugatsky freud gegel
  )))
)

(defrule literature-fiction-type "Тип художественной литературы"
  (favorite-liter fiction)
  (not (liter-type ?))
  (not (repair ?))
  =>
  (assert (liter-type (ask-question
    "Choose the type of literature (epic/drama)? "
    epic drama
  )))
)

(defrule literature-popular-type "Тип научно-популярной литературы"
  (favorite-liter popular)
  (not (liter-type ?))
  (not (repair ?))
  =>
  (assert (liter-type (ask-question
    "Choose the type of literature (psychology/science)? "
    psychology science
  )))
)

(defrule literature-fiction-genre "Жанр художественной литературы"
  (liter-type drama)
  (not (liter-genre ?))
  (not (repair ?))
  =>
  (assert (liter-genre (ask-question
    "Choose a genre of literature (roman/tragicomedy)? "
    roman tragicomedy
  )))
)

(defrule literature-popular-genre "Жанр научно-популярной литературы"
  (liter-type science)
  (not (liter-genre ?))
  (not (repair ?))
  =>
  (assert (liter-genre (ask-question
    "Choose a genre of literature (humanitarian/technical)? "
    humanitarian technical
  )))
)

;;;**********************
;;;* INTERMEDIATE RULES *
;;;**********************

;Определяем литературу
(defrule literature-fiction ""
  (or
    (favorite-book 1)
    (favorite-book 2)
  )
  (not (favorite-liter ?))
  (not (repair ?))
  =>
  (assert (favorite-liter fiction))
)
(defrule literature-popular ""
  (or
    (favorite-book 3)
    (favorite-book 4)
  )
  (not (favorite-liter ?))
  (not (repair ?))
  =>
  (assert (favorite-liter popular))
)

;Определяем жанр
(defrule literature-genre-roman ""
  (liter-type epic)
  (not (liter-genre ?))
  (not (repair ?))
  =>
  (assert (liter-genre roman))
)
(defrule literature-genre-psychology ""
  (liter-type psychology)
  (not (liter-genre ?))
  (not (repair ?))
  =>
  (assert (liter-genre humanitarian))
)

;;;****************
;;;* REPAIR RULES *
;;;****************

(defrule repair-hello-ficus ""
  (liter-genre roman)
  (favorite-writer orwell)
  (not (repair ?))
  =>
  (assert (repair "Keep the Aspidistra Flying"))
)

(defrule repair-point-conter-point ""
  (liter-genre roman)
  (favorite-writer strugatsky)
  (not (repair ?))
  =>
  (assert (repair "Point Counter Point"))
)

(defrule repair-forward-foundation ""
  (liter-genre roman)
  (favorite-writer freud)
  (not (repair ?))
  =>
  (assert (repair "Forward the Foundation"))
)

(defrule repair-or-or ""
  (liter-genre roman)
  (favorite-writer gegel)
  (not (repair ?))
  =>
  (assert (repair "Or-Or"))
)

(defrule repair-burmese-days ""
  (liter-genre tragicomedy)
  (favorite-writer orwell)
  (not (repair ?))
  =>
  (assert (repair "Burmese Days"))
)

(defrule repair-interns ""
  (liter-genre tragicomedy)
  (favorite-writer strugatsky)
  (not (repair ?))
  =>
  (assert (repair "Interns"))
)

(defrule repair-totem-taboo ""
  (liter-genre tragicomedy)
  (favorite-writer freud)
  (not (repair ?))
  =>
  (assert (repair "Totem and Taboo"))
)

(defrule repair-philosophy-right ""
  (liter-genre tragicomedy)
  (favorite-writer gegel)
  (not (repair ?))
  =>
  (assert (repair "Elements of the Philosophy of Right"))
)

(defrule repair-island ""
  (liter-genre humanitarian)
  (favorite-writer orwell)
  (not (repair ?))
  =>
  (assert (repair "Island"))
)

(defrule repair-kid-from-hell ""
  (liter-genre humanitarian)
  (favorite-writer strugatsky)
  (not (repair ?))
  =>
  (assert (repair "The Kid from Hell"))
)

(defrule repair-answer-to-job ""
  (liter-genre humanitarian)
  (favorite-writer freud)
  (not (repair ?))
  =>
  (assert (repair "Answer to Job"))
)

(defrule repair-philosophy-spirit ""
  (liter-genre humanitarian)
  (favorite-writer gegel)
  (not (repair ?))
  =>
  (assert (repair "Philosophy of spirit"))
)

(defrule repair-hacking-linux ""
  (liter-genre technical)
  (favorite-writer orwell)
  (not (repair ?))
  =>
  (assert (repair "Hacking on LINUX"))
)

(defrule repair-last-screw ""
  (liter-genre technical)
  (favorite-writer strugatsky)
  (not (repair ?))
  =>
  (assert (repair "Until the last screw"))
)

(defrule repair-multithreaded-js ""
  (liter-genre technical)
  (favorite-writer freud)
  (not (repair ?))
  =>
  (assert (repair "Multithreaded Javascript"))
)

(defrule repair-fender-protection ""
  (liter-genre technical)
  (favorite-writer gegel)
  (not (repair ?))
  =>
  (assert (repair "Fender protection"))
)

;;;********************************
;;;* STARTUP AND CONCLUSION RULES *
;;;********************************

(defrule system-banner ""
  (declare (salience 10))
  =>
  (printout t crlf crlf)
  (printout t "Book recommendation system")
  (printout t crlf crlf)
)

(defrule print-repair ""
  (declare (salience 10))
  (repair ?item)
  =>
  (printout t crlf crlf)
  (printout t "Recommended book:")
  (printout t crlf crlf)
  (format t " %s%n%n%n" ?item)
)
