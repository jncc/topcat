

# from http://programanddesign.com/js/jquery-select-text-range/
$.fn.highlightInputSelectionRange = (start, end) ->
    this.each () ->
        if this.setSelectionRange # non-IE
            this.focus()
            this.setSelectionRange start, end
        else if this.createTextRange # IE
            range = this.createTextRange()
            range.collapse true
            range.moveEnd 'character', end
            range.moveStart 'character', start
            range.select()




