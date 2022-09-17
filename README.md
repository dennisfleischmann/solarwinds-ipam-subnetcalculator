# solarwinds-ipam-subnetcalculator


Linux


Example Usage

```
./SubnetCalculator $supernet $cider $subnetCider $username $password $host | grep rows | python -m json.tool
```

Response

```
{
    "count": 4,
    "rows": [
        [
            "10.44.0.128",
            "10.44.0.159",
            "255.255.255.224"
        ],
        [
            "10.44.0.160",
            "10.44.0.191",
            "255.255.255.224"
        ],
        [
            "10.44.0.192",
            "10.44.0.223",
            "255.255.255.224"
        ],
        [
            "10.44.0.224",
            "10.44.0.255",
            "255.255.255.224"
        ]
    ]
}
```
 