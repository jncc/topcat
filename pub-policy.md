
GetPublishingPolicyResult

Record ->

PublishingPolicyResult {
  Message:      string
  HubRecord:    bool
  HubResources: string[]
  GovRecord:    bool
  GovResources: string[]
}

If publishable
   // Special cases
   If darwin+ DGU // keyword(s)
   {
    Message:      "This is a Darwin+ record we're contracted to publish..."
    HubRecord:    false
    GovRecord:    true
    GovResources: resources
   }
   Else if DOI that has already been published
   {
    Message:      "This record has a DOI and has cannot be republished."
    HubRecord:    false
    GovRecord:    false
   }
   Else if OpenDataIssue (Licencing,WhatElse?)
   {
    Message:      "This record has an Open Data issue:"
    HubRecord:    true
    HubResources: []     // none!
    GovRecord:    false
   }
   Else if Publication
   {
    Message:      "This a JNCC publication."
    HubRecord:    true
    HubResources: []
    GovRecord:    false
   }
   Else  // includes DOI first publish
   {
    Message:   "This an Open Data record."
    HubRecord:    true
    HubResources: resources
    GovRecord:        true
   }
Else
{
 Message:      "This record has not been marked as publishable"
 HubRecord:    false
 HubResources: []
 GovRecord:    false
}
