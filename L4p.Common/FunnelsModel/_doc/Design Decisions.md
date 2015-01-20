## Funnels design decisions

- Data inside funnel is only updated (previous instances are stored as well)
- Funnel stores serialized representation of data, thus changing returned instances has no effect until data is explicitly published
- Two different instances of a funnel are only eventually consistent even inside a same process
- FunnelsManager tries to keep in-process instances of a funnel consistent
