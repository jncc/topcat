(function() {

  $.fn.highlightInputSelectionRange = function(start, end) {
    return this.each(function() {
      var range;
      if (this.setSelectionRange) {
        this.focus();
        return this.setSelectionRange(start, end);
      } else if (this.createTextRange) {
        range = this.createTextRange();
        range.collapse(true);
        range.moveEnd('character', end);
        range.moveStart('character', start);
        return range.select();
      }
    });
  };

}).call(this);
