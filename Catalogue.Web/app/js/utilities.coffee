

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


String.prototype.hashCode = () ->
    hash = 0 #, i, char;
    if (this.length == 0)
        hash
    else
        for element, i in this
            #   for (i = 0, l = this.length; i < l; i++) {
            char = this.charCodeAt(i)
            hash = ((hash << 5) - hash) + char
            hash |= 0 #; // Convert to 32bit integer
        hash

